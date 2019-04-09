using System;
using System.Threading;
using System.Threading.Tasks;
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

                await connection.StartAsync(cancellationToken);

                Task OnClosed(Exception e)
                {
                    // reconnect
                    return connection.StartAsync(cancellationToken);
                }

                connection.Closed += OnClosed;
                cancellationToken.Register(() =>
                {
                    // close connection
                    connection.Closed -= OnClosed;
                    var _ = connection.StopAsync(CancellationToken.None);
                });

                return connection;
            });
        }
    }
}