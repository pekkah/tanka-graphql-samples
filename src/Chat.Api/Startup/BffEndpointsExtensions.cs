using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Tanka.GraphQL.Samples.Chat.Api.Startup;

public static class BffEndpointsExtensions
{
    public static WebApplication UseBffEndpoints(this WebApplication app)
    {
        app.MapGet("/signin",
            async context =>
                await context.ChallengeAsync("github", new OAuthChallengeProperties { RedirectUri = "/" }));

        app.MapGet("/signout",
            async context =>
                await context.SignOutAsync("Cookies", new AuthenticationProperties { RedirectUri = "/" }));

        app.MapGet("/session", async context =>
        {
            if (context.User.Identity?.IsAuthenticated == false)
                await context.Response.WriteAsJsonAsync(new { IsAuthenticated = false });
            else
                await context.Response.WriteAsJsonAsync(new
                {
                    IsAuthenticated = true,
                    Name = context.User.FindFirstValue("name"),
                    AvatarUrl = context.User.FindFirstValue("avatar_url"),
                    Login = context.User.FindFirstValue("login"),
                    Id = context.User.FindFirstValue("id")
                });
        });

        return app;
    }
}