using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

using AspNet.Security.OAuth.GitHub;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Octokit;

namespace Tanka.GraphQL.Samples.Chat.Api;

public partial class
    GitHubAccessTokenAuthenticationHandler : AuthenticationHandler<GitHubAccessTokenAuthenticationOptions>
{
    public GitHubAccessTokenAuthenticationHandler(
        IOptionsMonitor<GitHubAccessTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        Backchannel = new HttpClient();
    }


    public HttpClient Backchannel { get; set; }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? accessToken = GetAccessToken();

        if (accessToken is null)
            return AuthenticateResult.NoResult();

        try
        {
            var github = new GitHubClient(new ProductHeaderValue("tanka-chat-api"));
            github.Credentials = new Credentials(accessToken, AuthenticationType.Oauth);
            User? user = await github.User.Current();

            var identity = new ClaimsIdentity(new Claim[]
            {
                new(JwtRegisteredClaimNames.Name, user.Name), new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("avatar_url", user.AvatarUrl)
            });
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal!, new AuthenticationProperties(), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception x)
        {
            Logger.LogError(x, "Failed to fetch user info");
            return AuthenticateResult.Fail("User info not available.");
        }
    }

    private string? GetAccessToken()
    {
        if (Request.Headers.TryGetValue("authorization", out StringValues values))
        {
            string? accessToken = values.First();
            accessToken = accessToken?.Replace("Bearer ", string.Empty).Trim();

            if (accessToken is { Length: > 0 })
                return accessToken;
        }

        if (Request.Query.TryGetValue("a", out values))
        {
            string? accessToken = values.First();
            accessToken = accessToken?.Replace("Bearer ", string.Empty).Trim();

            if (accessToken is { Length: > 0 })
                return accessToken;
        }

        return null;
    }

    private static partial class Log
    {
        internal static async Task EmailAddressErrorAsync(ILogger logger, HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            EmailAddressError(
                logger,
                response.StatusCode,
                response.Headers.ToString(),
                await response.Content.ReadAsStringAsync(cancellationToken));
        }

        internal static async Task UserProfileErrorAsync(ILogger logger, HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            UserProfileError(
                logger,
                response.StatusCode,
                response.Headers.ToString(),
                await response.Content.ReadAsStringAsync(cancellationToken));
        }

        [LoggerMessage(2, LogLevel.Warning,
            "An error occurred while retrieving the email address associated with the logged in user: the remote server returned a {Status} response with the following payload: {Headers} {Body}.")]
        private static partial void EmailAddressError(
            ILogger logger,
            HttpStatusCode status,
            string headers,
            string body);

        [LoggerMessage(1, LogLevel.Error,
            "An error occurred while retrieving the user profile: the remote server returned a {Status} response with the following payload: {Headers} {Body}.")]
        private static partial void UserProfileError(
            ILogger logger,
            HttpStatusCode status,
            string headers,
            string body);
    }
}

public class GitHubAccessTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public string UserInformationEndpoint { get; set; } = GitHubAuthenticationDefaults.UserInformationEndpoint;

    public string UserEmailsEndpoint { get; set; } = GitHubAuthenticationDefaults.UserEmailsEndpoint;
}