using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace tanka.graphql.samples.messages.host.logic
{
    public class MutationController : MutationControllerBase<Mutation>
    {
        private readonly Messages _messages;

        public MutationController(Messages messages)
        {
            _messages = messages;
        }

        public override async ValueTask<Message> PostMessage(
            Mutation? objectValue, 
            int channelId, 
            InputMessage message,
            IResolverContext context)
        {
            // current user is being injected by the resolver middleware
            var user = (ClaimsPrincipal) context.Items["user"];

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