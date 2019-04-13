using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.links;

namespace tanka.graphql.samples.Host
{
    public static class Links
    {
        public static ExecutionResultLink SignalR(string url)
        {
            return RemoteLinks.Server(async cancellationToken =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

                return connection;
            });
        }
    }
}