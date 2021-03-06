﻿using System;
using System.Linq;
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
using Microsoft.IdentityModel.Tokens;
using Tanka.GraphQL.Extensions.Analysis;
using tanka.graphql.samples.channels.host.logic;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.Links.DTOs;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace tanka.graphql.samples.channels.host
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
            services.AddControllers()
                .AddJsonOptions(options =>
            {
                // required to serialize 
                options.JsonSerializerOptions.Converters
                    .Add(new ObjectDictionaryConverter());
            });

            AddForwardedHeaders(services);

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
                        var accessToken = context.Request.Query["access_token"]
                            .ToString();

                        if (string.IsNullOrEmpty(accessToken))
                            accessToken = context.Request
                                .Headers["Authorization"]
                                .ToString()
                                .Replace("Bearer ", string.Empty);


                        // Read the token out of the query string
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
                options.AddPolicy("authorize", policy => policy.RequireAuthenticatedUser()));

            services.AddHttpContextAccessor();

            // add domain
            services.AddSingleton<Channels>();

            // add schema
            services.AddSchemaControllers()
                .AddQueryController<QueryController>()
                .AddChannelController<ChannelController>();

            services.AddMemoryCache();
            services.AddSingleton<SchemaCache>();

            services.AddTankaGraphQL()
                .ConfigureSchema<SchemaCache>(async cache => await cache.GetOrAdd())
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

                routes.MapControllers();
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