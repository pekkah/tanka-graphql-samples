using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server.Links;
using Tanka.GraphQL.Server.Links.DTOs;

namespace tanka.graphql.samples.Host
{
    public static class Links
    {
        public static ExecutionResultLink SignalR(string url, IHttpContextAccessor accessor)
        {
            return RemoteLinks.SignalR(cancellationToken =>
            {
                var connection = new HubConnectionBuilder()
                    .AddJsonProtocol(options =>
                    {
                        options
                            .PayloadSerializerOptions
                            .Converters
                            .Add(new ObjectDictionaryConverter());
                    })
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
                    .AddJsonProtocol(options =>
                    {
                        options
                            .PayloadSerializerOptions
                            .Converters
                            .Add(new ObjectDictionaryConverter());
                    })
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
                },
                transformResponse: response =>
                {
                    return HttpLink.DefaultTransformResponse(response);
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