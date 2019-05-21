using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
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
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;
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
            services.AddAuthentication()
                .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
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
                            {
                                accessToken = messageContext.Context?.Message
                                    .Payload
                                    .SelectToken("authorization")
                                    .ToString();
                            }
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

                            context.Principal.AddIdentity(tokenIdentity);
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

            // add schema
            services.AddSingleton(
                provider =>
                {
                    try
                    {
                        // used to get the access token
                        var accessor = provider.GetRequiredService<IHttpContextAccessor>();

                        // create channelsSchema by introspecting channels service
                        var channelsLink = Links.SignalROrHttp(
                            Configuration["Remotes:Channels"], 
                            Configuration["Remotes:ChannelsHttp"],
                            accessor);

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

            // add execution options
            services.AddTankaExecutionOptions()
                .Configure<IHttpContextAccessor>((options, accessor) => options.GetSchema 
                    = query => new ValueTask<ISchema>(accessor
                        .HttpContext
                        .RequestServices
                        .GetRequiredService<ISchema>()));


            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddTankaServerHubWithTracing();


            services.AddTankaWebSocketServerWithTracing()
                .Configure<IHttpContextAccessor>(
                    (options, 
                        accessor
                    ) => options.AcceptAsync = async context =>
                    {
                        var succeeded = await AuthorizeHelper.AuthorizeAsync(
                            accessor.HttpContext,
                            new List<IAuthorizeData>()
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
            app.UseSignalR(routes => routes.MapTankaServerHub("/hubs/graphql",
                options =>
                {
                    options.AuthorizationData.Add(new AuthorizeAttribute("authorize"));
                }));

            // use websockets server
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