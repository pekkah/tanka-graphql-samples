using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.links;

namespace tanka.graphql.samples.Host
{
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