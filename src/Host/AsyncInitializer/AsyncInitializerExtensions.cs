using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace fugu.graphql.samples.Host.AsyncInitializer
{
    public static class AsyncInitializerExtensions
    {
        public static Task InitializeAsyncServices(this IWebHost host)
        {
            var asyncServices = host.Services.GetServices<IAsyncInitializer>();

            return Task.WhenAll(asyncServices.Select(service => service.InitializeAsync()));
        }
    }
}