using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tanka.GraphQL;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.Links.DTOs;

namespace tanka.graphql.samples.channels.host.api
{
    [Authorize(Policy = "authorize")]
    [Route("api/graphql")]
    [ApiController]
    public class QueryController : Controller
    {
        private readonly IQueryStreamService _queryStreamService;

        public QueryController(IQueryStreamService queryStreamService)
        {
            _queryStreamService = queryStreamService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QueryRequest request)
        {
            var document = Parser.ParseDocument(request.Query);
            var stream = await _queryStreamService.QueryAsync(new Query
            {
                Document = document,
                Variables = request.Variables,
                OperationName = request.OperationName,
            }, Request.HttpContext.RequestAborted);

            var result = await stream.Reader.ReadAsync();

            return Ok(result);
        }
    }
}