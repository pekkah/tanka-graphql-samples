using System.Security.Claims;

using Microsoft.AspNetCore.Http.Features;

using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Api;

public static class ResolverContextExtensions
{
    public static ClaimsPrincipal GetUser(this ResolverContext context)
    {
        return GetHttpContext(context).User;
    }

    public static HttpContext GetHttpContext(this ResolverContext context)
    {
        return context.QueryContext.Features.GetRequiredFeature<IHttpContextFeature>().HttpContext;
    }
}