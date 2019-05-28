using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using tanka.graphql.analysis;
using tanka.graphql.samples.messages.host.logic;
using tanka.graphql.server;
using tanka.graphql.tools;
using tanka.graphql.type;
using tanka.graphql.validation;

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
                                .GetOrCreateAsync<ClaimsIdentity>(key, async entry =>
                                {
                                    entry.SetAbsoluteExpiration(jwt.ValidTo -TimeSpan.FromSeconds(5));

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

            // add schema
            services.AddSingleton<Messages>();
            services.AddSingleton<ResolverService>();
            services.AddSingleton<Resolvers>();
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
                        foreach (var field in connect.VisitFields(mutation))
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
                    var resolvers = provider.GetRequiredService<Resolvers>();
                    var schema = SchemaTools.MakeExecutableSchemaWithIntrospection(
                        schemaBuilder,
                        resolvers,
                        resolvers);

                    return schema;
                });

            services.AddTankaSchemaOptions()
                .Configure<IHttpContextAccessor>((options, accessor) =>
                {
                    options.ValidationRules = ExecutionRules.All
                        .Concat(new[]
                        {
                            CostAnalyzer.MaxCost(
                                maxCost: 100,
                                defaultFieldComplexity: 1
                            )
                        }).ToArray();

                    options.GetSchema
                        = query => new ValueTask<ISchema>(accessor
                            .HttpContext
                            .RequestServices
                            .GetRequiredService<ISchema>());
                });

            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddTankaServerHubWithTracing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            // use signalr server hub
            app.UseSignalR(routes => routes.MapTankaServerHub("/hubs/graphql",
                options => { options.AuthorizationData.Add(new AuthorizeAttribute("authorize")); }));
        }
    }
}