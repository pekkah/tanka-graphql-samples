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
            return RemoteLinks.SignalR(cancellationToken =>
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
            var signalr = RemoteLinks.SignalR(cancellationToken =>
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
                    var request = HttpLink.DefaultTransformRequest(operation);
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        accessor.HttpContext?.User.FindFirstValue("access_token"));

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