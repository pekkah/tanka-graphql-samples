using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using tanka.graphql.introspection;
using tanka.graphql.server;
using tanka.graphql.tools;
using tanka.graphql.type;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            // signalr authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
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
                    try
                    {
                        // used to get the access token
                        var accessor = provider.GetRequiredService<IHttpContextAccessor>();

                        // create channelsSchema by introspecting channels service
                        var channelsLink = Links.SignalR(Configuration["Remotes:Channels"], accessor);
                        var channelsSchema = RemoteSchemaTools.MakeRemoteExecutable(
                            new SchemaBuilder()
                                .ImportIntrospectedSchema(channelsLink)
                                .ConfigureAwait(false)
                                .GetAwaiter().GetResult(),
                            channelsLink);

                        // create messagesSchema by introspecting messages service
                        var messagesLink = Links.SignalR(Configuration["Remotes:Messages"], accessor);
                        var messagesSchema = RemoteSchemaTools.MakeRemoteExecutable(
                            new SchemaBuilder()
                                .ImportIntrospectedSchema(messagesLink)
                                .ConfigureAwait(false)
                                .GetAwaiter().GetResult(),
                            messagesLink);

                        // combine schemas into one
                        var schema = new SchemaBuilder()
                            .Merge(channelsSchema, messagesSchema)
                            .Build();

                        // introspect and merge with schema
                        var introspection = Introspect.Schema(schema);
                        return new SchemaBuilder()
                            .Merge(schema, introspection)
                            .Build();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });

            // add signalr
            /*services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddQueryStreamHubWithTracing();*/

            services.AddTankaExecutionOptions()
                .Configure<ISchema>((options, schema) => options.Schema = schema);

            services.AddTankaWebSocketServerWithTracing();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
            app.UseSpaStaticFiles();

            // use authenication
            app.UseAuthentication();

            // use signalr server hub
            app.UseWebSockets();
            app.UseTankaWebSocketServer(new WebSocketServerOptions()
            {
                Path = "/api/graphql"
            });

            // use mvc
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment()) spa.UseReactDevelopmentServer("start");
            });
        }
    }
}