using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Tanka.GraphQL.Extensions.Analysis;
using tanka.graphql.samples.messages.host.logic;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace tanka.graphql.samples.messages.host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            AddForwardedHeaders(services);

            services.AddMemoryCache();

            // signalr authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
                options.Authority = Configuration["JWT:Authority"];
                options.Audience = Configuration["JWT:Audience"];
                options.SaveToken = true;

                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // Read the token out of the query string
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        if (context.SecurityToken is JwtSecurityToken jwt)
                        {
                            /*
                                PERFORMANCE/SECURITY
                             
                                For performance reasons we're caching the results of the userInfo request.

                                This is NOT production quality and you should carefully think when using 
                                it in your application
                            */
                            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                            var key = jwt.Claims.Single(c => c.Type == "sub").Value;
                            var userInfoClaimsIdentity = await cache
                                .GetOrCreateAsync(key, async entry =>
                                {
                                    entry.SetAbsoluteExpiration(jwt.ValidTo - TimeSpan.FromSeconds(5));

                                    var configuration = await context.Options.ConfigurationManager
                                        .GetConfigurationAsync(CancellationToken.None);

                                    var userInfoEndpoint = configuration.UserInfoEndpoint;
                                    var client = new HttpClient();
                                    var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
                                    {
                                        Address = userInfoEndpoint,
                                        Token = jwt.RawData
                                    });

                                    if (userInfo.IsError)
                                        return new ClaimsIdentity();

                                    return new ClaimsIdentity(userInfo.Claims);
                                });

                            context.Principal.AddIdentity(userInfoClaimsIdentity);
                        }
                    }
                };
            });

            services.AddAuthorization(options =>
                options.AddPolicy("authorize", policy => policy.RequireAuthenticatedUser()));

            services.AddHttpContextAccessor();

            // add domain
            services.AddSingleton<Messages>();
            
            // add schema
            services.AddSchemaControllers()
                .AddQueryController<QueryController>()
                .AddMutationController<MutationController>()
                .AddSubscriptionController<SubscriptionController>()
                .AddMessageController<MessageController>();

            services.AddSingleton(
                provider =>
                {
                    // load typeDefs
                    var schemaBuilder = SchemaLoader.Load();

                    // add current user to arguments of resolvers of mutation fields
                    var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                    schemaBuilder.TryGetType<ObjectType>("Mutation", out var mutation);
                    schemaBuilder.Connections(connect =>
                    {
                        foreach (var field in connect.GetFields(mutation))
                        {
                            var resolver = connect.GetOrAddResolver(mutation, field.Key);
                            resolver.Use((context, next) =>
                            {
                                context.Items["user"] = accessor.HttpContext?.User;
                                return next(context);
                            });
                        }
                    });

                    // bind the actual field value resolvers and create schema
                    var resolvers = new SchemaResolvers();
                    var schema = SchemaTools.MakeExecutableSchemaWithIntrospection(
                        schemaBuilder,
                        resolvers,
                        resolvers);

                    return schema;
                });

            services.AddTankaGraphQL()
                .ConfigureSchema<IHttpContextAccessor>(accessor => new ValueTask<ISchema>(accessor
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<ISchema>()))
                .ConfigureRules(rules => rules.Concat(new[]
                {
                    CostAnalyzer.MaxCost(
                        100
                    )
                }).ToArray());

            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddTankaGraphQL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // use signalr server hub
            app.UseEndpoints(routes =>
            {
                routes.MapTankaGraphQLSignalR("/hubs/graphql",
                    options =>
                    {
                        options.AuthorizationData.Add(new AuthorizeAttribute("authorize"));
                    });
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