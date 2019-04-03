using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using tanka.graphql.samples.Chat.Host.Schemas;
using tanka.graphql.server;
using tanka.graphql.tools;

namespace tanka.graphql.samples.Chat.Host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // add schema
            services.AddSingleton(
                provider =>
                {
                    var schemaBuilder = SchemaLoader.Load();

                    var chat = new Domain.Chat();
                    var service = new ChatResolverService(chat);
                    var resolvers = new ChatResolvers(service);

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
