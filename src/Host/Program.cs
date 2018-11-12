using System.Threading.Tasks;
using fugu.graphql.samples.Host.AsyncInitializer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace fugu.graphql.samples.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            await host.InitializeAsyncServices();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
