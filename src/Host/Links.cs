using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.links;

namespace tanka.graphql.samples.Host
{
    public static class Links
    {
        public static ExecutionResultLink SignalR(string url)
        {
            return RemoteLinks.Server(cancellationToken =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

                return Task.FromResult(connection);
            });
        }
    }
}