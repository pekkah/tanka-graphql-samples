using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Tanka.GraphQL.Samples.Chat.Api.Startup;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddCookieAndGitHubAuthentication(this WebApplicationBuilder builder)
    {
        // breaking change from previous NET versions: clear the default mappings to soap claim names
        JsonWebTokenHandler.DefaultMapInboundClaims = false;
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddGitHub("github", options =>
            {
                options.ClientId = builder.Configuration["GitHub:ClientId"] ?? throw new InvalidOperationException();
                options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ??
                                       throw new InvalidOperationException();
                options.Scope.Add("user:email");

                options.ClaimActions.Clear();
                options.ClaimActions.MapJsonKey("name", "name");
                options.ClaimActions.MapJsonKey("id", "id");
                options.ClaimActions.MapJsonKey("login", "login");
                options.ClaimActions.MapJsonKey("avatar_url", "avatar_url");
                options.ClaimActions.MapJsonKey("email", "email");
            });

        return builder;
    }
}