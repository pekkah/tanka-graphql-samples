using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // add schema
            services.AddSingleton(
                provider =>
                {
                    // create channelsSchema by introspecting channels service
                    var channelsLink = Links.SignalR(Configuration["Remotes:Channels"]);
                    var channelsSchema = RemoteSchemaTools.MakeRemoteExecutable(
                        new SchemaBuilder()
                            .ImportIntrospectedSchema(channelsLink).GetAwaiter().GetResult(),
                        channelsLink);

                    // create messagesSchema by introspecting messages service
                    var messagesLink = Links.SignalR(Configuration["Remotes:Messages"]);
                    var messagesSchema = RemoteSchemaTools.MakeRemoteExecutable(
                        new SchemaBuilder()
                            .ImportIntrospectedSchema(messagesLink).GetAwaiter().GetResult(),
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
}