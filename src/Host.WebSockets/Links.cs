using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.links;

namespace tanka.graphql.samples.Host
{
    public static class Links
    {
        public static ExecutionResultLink SignalR(string url, IHttpContextAccessor accessor)
        {
            return RemoteLinks.Server(cancellationToken =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(url, configure =>
                    {
                        configure.AccessTokenProvider = async () =>
                        {
                            var token = accessor.HttpContext.User
                                .FindFirstValue("access_token");

                            return token;
                        };
                    })
                    .Build();

                return Task.FromResult(connection);
            });
        }
    }
}