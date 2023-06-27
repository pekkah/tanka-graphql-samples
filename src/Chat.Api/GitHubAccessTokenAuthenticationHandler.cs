using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

using AspNet.Security.OAuth.GitHub;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Octokit;

using ProductHeaderValue = Octokit.ProductHeaderValue;

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


    protected virtual async Task<string?> GetEmailAsync(string accessToken)
    {
        // See https://developer.github.com/v3/users/emails/ for more information about the /user/emails endpoint.
        using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserEmailsEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage response =
            await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            await Log.EmailAddressErrorAsync(Logger, response, Context.RequestAborted);
            throw new HttpRequestException(
                "An error occurred while retrieving the email address associated to the user profile.");
        }

        using var payload =
            JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

        return (from address in payload.RootElement.EnumerateArray()
            where address.GetProperty("primary").GetBoolean()
            select address.GetString("email")).FirstOrDefault();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("authorization", out StringValues values))
            return AuthenticateResult.NoResult();

        string? accessToken = values.First();
        if (accessToken is null)
            return AuthenticateResult.NoResult();

        accessToken = accessToken.Replace("Bearer ", string.Empty).Trim();
        try
        {
            var github = new GitHubClient(new ProductHeaderValue("tanka-chat-api"));
            github.Credentials = new Credentials(accessToken, AuthenticationType.Oauth);
            User? user = await github.User.Current();

            var identity = new ClaimsIdentity(new Claim[]
            {
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
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