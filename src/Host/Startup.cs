using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace tanka.graphql.samples.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            AddForwardedHeaders(services);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddCors(cors =>
            {
                cors.AddDefaultPolicy(policy =>
                {
                    policy.SetIsOriginAllowed(origin => true);
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                });
            });

            // signalr authentication
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };

                    options.Authority = Configuration["JWT:Authority"];
                    options.Audience = Configuration["JWT:Audience"];
                    options.SaveToken = false;

                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or 
                    // Server-Sent Events request comes in.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"].ToString();

                            if (string.IsNullOrEmpty(accessToken))
                            {
                                var messageContext = context.HttpContext
                                    .RequestServices.GetRequiredService<IMessageContextAccessor>();

                                var message = messageContext.Context?.Message;

                                if (message?.Type == MessageType.GQL_CONNECTION_INIT)
                                    if (messageContext.Context?.Message.Payload is Dictionary<string, object> payload
                                        && payload.ContainsKey("authorization"))
                                        accessToken = payload["authorization"].ToString()!;
                            }

                            // Read the token out of the query string
                            context.Token = accessToken;
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            if (context.SecurityToken is JwtSecurityToken jwt)
                            {
                                var tokenIdentity = new ClaimsIdentity(
                                    new[]
                                    {
                                        new Claim("access_token", jwt.RawData)
                                    });

                                context.Principal?.AddIdentity(tokenIdentity);
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
                options.AddPolicy("authorize",
                    policy => policy
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()));

            services.AddHttpContextAccessor();

            // add schema cache
            services.AddSingleton<SchemaCache>();
            services.AddMemoryCache();

            // add execution options
            services.AddTankaGraphQL()
                .ConfigureSchema<SchemaCache>(async cache => await cache.GetOrAdd())
                .ConfigureRules<ILogger<Startup>>((rules, logger) => rules.Concat(new[]
                {
                    CostAnalyzer.MaxCost(
                        100,
                        1,
                        onCalculated: operation =>
                        {
                            logger.LogInformation(
                                $"Operation '{operation.Operation.ToGraphQL()}' costs " +
                                $"'{operation.Cost}' (max: '{operation.MaxCost}')");
                        }
                    )
                }).ToArray())
                .ConfigureWebSockets<IHttpContextAccessor>(
                    async (context, accessor) =>
                    {
                        var succeeded = await AuthorizeHelper.AuthorizeAsync(
                            accessor.HttpContext,
                            new List<IAuthorizeData>
                            {
                                new AuthorizeAttribute("authorize")
                            });

                        if (succeeded)
                        {
                            await context.Output.WriteAsync(new OperationMessage
                            {
                                Type = MessageType.GQL_CONNECTION_ACK
                            });
                        }
                        else
                        {
                            // you must decide what kind of message to send back to the client
                            // in case the connection is not accepted.
                            await context.Output.WriteAsync(new OperationMessage
                            {
                                Type = MessageType.GQL_CONNECTION_ERROR,
                                Id = context.Message.Id
                            });

                            // complete the output forcing the server to disconnect
                            context.Output.Complete();
                        }
                    });


            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddTankaGraphQL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            app.UseCors();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // use websockets server
            app.UseWebSockets();

            app.UseEndpoints(routes =>
            {
                routes.MapTankaGraphQLSignalR("/hubs/graphql",
                    options => { options.AuthorizationData.Add(new AuthorizeAttribute("authorize")); });

                routes.MapTankaGraphQLWebSockets("/api/graphql");
            });
        }

        private void AddForwardedHeaders(IServiceCollection services)
        {
            if (string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"),
                "true", StringComparison.OrdinalIgnoreCase))
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                               ForwardedHeaders.XForwardedProto;
                    // Only loopback proxies are allowed by default.
                    // Clear that restriction because forwarders are enabled by explicit 
                    // configuration.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
        }
    }
}