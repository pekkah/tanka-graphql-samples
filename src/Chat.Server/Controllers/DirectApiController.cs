using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Tanka.GraphQL.Samples.Chat.Server.Controllers;

[ValidateAntiForgeryToken]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class DirectApiController : ControllerBase
{
    [HttpGet]
    public IEnumerable<string> Get() => new List<string> { "some data", "more data", "loads of data" };
}
