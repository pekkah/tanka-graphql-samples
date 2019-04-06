using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using tanka.graphql.samples.channels.host.logic;
using tanka.graphql.server;
using tanka.graphql.tools;

namespace tanka.graphql.samples.channels.host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
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

            // add signalr
            services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                .AddQueryStreamHubWithTracing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // use fugu signalr server hub
            app.UseSignalR(routes => routes.MapHub<QueryStreamHub>("/hubs/graphql"));
        }
    }
}
