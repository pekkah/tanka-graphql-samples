using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using tanka.graphql.analysis;
using tanka.graphql.samples.channels.host.logic;
using tanka.graphql.server;
using tanka.graphql.tools;
using tanka.graphql.type;
using tanka.graphql.validation;

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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

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

            // add schema
            services.AddSingleton(
                provider =>
                {
                    var schemaBuilder = SchemaLoader.Load();

                    var chat = new Channels();
                    var service = new ResolverService(chat);
                    var resolvers = new Resolvers(service);

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
                                100
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
            app.UseForwardedHeaders();

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            // use signalr server hub
            app.UseSignalR(routes => routes.MapTankaServerHub("/hubs/graphql",
                options => { options.AuthorizationData.Add(new AuthorizeAttribute("authorize")); }));

            app.UseMvc();
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