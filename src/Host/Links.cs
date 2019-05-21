using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using tanka.graphql.language;
using tanka.graphql.links;
using tanka.graphql.requests;

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
                        configure.AccessTokenProvider = () =>
                        {
                            var token = accessor.HttpContext
                                ?.User
                                .FindFirstValue("access_token");

                            return Task.FromResult(token);
                        };
                    })
                    .Build();

                return Task.FromResult(connection);
            });
        }

        public static ExecutionResultLink SignalROrHttp(
            string hubUrl,
            string httpUrl,
            IHttpContextAccessor accessor)
        {
            var signalr = RemoteLinks.Server(cancellationToken =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, configure =>
                    {
                        configure.AccessTokenProvider = () =>
                        {
                            var token = accessor.HttpContext
                                ?.User
                                .FindFirstValue("access_token");

                            return Task.FromResult(token);
                        };
                    })
                    .Build();

                return Task.FromResult(connection);
            });

            var http = RemoteLinks.Http(
                httpUrl,
                transformRequest: operation =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, operation.Url);
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        accessor.HttpContext?.User.FindFirstValue("access_token"));

                    var query = new QueryRequest
                    {
                        Query = operation.Document.ToGraphQL(),
                        Variables = operation.Variables != null
                            ? new Dictionary<string, object>(operation.Variables)
                            : null
                    };
                    var json = JsonConvert.SerializeObject(query);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    return request;
                });

            return async (document, variables, token) =>
            {
                var hasQueryOrMutation = document.Definitions.OfType<GraphQLOperationDefinition>()
                    .Any(op => op.Operation != OperationType.Subscription);

                if (hasQueryOrMutation)
                {
                    var result = await http(document, variables, CancellationToken.None);
                    return result;
                }

                return await signalr(document, variables, token);
            };
        }
    }
}