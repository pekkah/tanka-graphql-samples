using Tanka.GraphQL.Samples.Chat.Shared.Defaults;

namespace Tanka.GraphQL.Samples.Chat.Client.Services;

public interface IAntiforgeryHttpClientFactory
{
    Task<HttpClient> CreateClientAsync(string clientName = AuthDefaults.AuthorizedClientName);
}
