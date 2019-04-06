using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using tanka.graphql.language;
using tanka.graphql.links;
using tanka.graphql.requests;

namespace tanka.graphql.samples.Host
{
    public static class Links
    {
        public static ExecutionResultLink Signalr(string url)
        {
            return async (document, variables, token) =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

                await connection.StartAsync(token);

                Task OnClosed(Exception e)
                {
                    return connection.StartAsync(token);
                }

                connection.Closed += OnClosed;
                token.Register(() => { connection.Closed -= OnClosed; });

                var query = new QueryRequest
                {
                    Query = document.ToGraphQL(),
                    Variables = variables != null
                        ? new Dictionary<string, object>(variables)
                        : null
                };

                return await connection.StreamQueryAsync(query, token);
            };
        }
    }
}