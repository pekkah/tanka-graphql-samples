using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class MutationController : MutationControllerBase<Mutation>
    {
        private readonly Messages _messages;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MutationController(Messages messages, IHttpContextAccessor httpContextAccessor)
        {
            _messages = messages;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<Message> PostMessage(
            Mutation? objectValue, 
            int channelId, 
            InputMessage message,
            IResolverContext context)
        {
            // current user
            var user = _httpContextAccessor.HttpContext.User;

            // use name claim from the profile if present otherwise use default name claim (sub)
            var from = user.FindFirstValue("name") ?? user.Identity.Name;

            if (string.IsNullOrEmpty(from))
                throw new InvalidOperationException(
                    "Could not find user name claim");

            // use profile picture claim from the profile if present otherwise leave empty
            var picture = user.FindFirstValue("picture") ?? string.Empty;

            return await _messages.PostMessageAsync(channelId, from, picture, message);
        }
    }
}
