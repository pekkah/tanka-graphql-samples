using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Tanka.GraphQL.Samples.Chat.Api.Startup;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddCookieAndGitHubAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = "github";
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
            })
            .AddGitHub("github", options =>
            {
                options.ClientId = builder.Configuration["GitHub:ClientId"] ?? throw new InvalidOperationException();
                options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ??
                                       throw new InvalidOperationException();
                options.Scope.Add("user:email");

                options.ClaimActions.MapAll();
            });

        return builder;
    }
}