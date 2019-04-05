using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using tanka.graphql.introspection;
using tanka.graphql.language;
using tanka.graphql.links;
using tanka.graphql.requests;
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

            // add schema
            services.AddSingleton(
                provider =>
                {
                    var builder = new SchemaBuilder();

                    // build schema by introspecting the chat schema
                    var link = Links.Signalr("https://localhost:5010/hubs/graphql");
                    builder.ImportIntrospectedSchema(link).GetAwaiter().GetResult();

                    return RemoteSchemaTools.MakeRemoteExecutable(
                        builder,
                        link);
                });

            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddQueryStreamHubWithTracing();

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

            // use signalr server hub
            app.UseSignalR(routes => routes.MapHub<QueryStreamHub>("/hubs/graphql"));

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

    public static class Links
    {
        public static ExecutionResultLink Signalr(string url)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            connection.StartAsync().GetAwaiter().GetResult();

            return RemoteLinks.Server(connection);
        }
    }
}