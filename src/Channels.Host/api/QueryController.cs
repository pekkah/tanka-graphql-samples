using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using tanka.graphql.requests;

namespace tanka.graphql.samples.channels.host.api
{
    [Authorize(Policy = "authorize")]
    [Route("api/graphql")]
    [ApiController]
    public class QueryController : Controller
    {
        private readonly IEnumerable<IExtension> _extensions;

        //todo: fix duplicate class name
        private readonly IOptionsMonitor<server.ExecutionOptions> _optionsMonitor;

        public QueryController(IOptionsMonitor<server.ExecutionOptions> optionsMonitor,
            IEnumerable<IExtension> extensions)
        {
            _optionsMonitor = optionsMonitor;
            _extensions = extensions;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QueryRequest request)
        {
            var options = _optionsMonitor.CurrentValue;

            var document = Parser.ParseDocument(request.Query);
            var schema = await options.GetSchema(null);
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = document,
                Schema = schema,
                OperationName = request.OperationName,
                VariableValues = request.Variables,
                Extensions = _extensions.ToArray()
            });

            return Ok(result);
        }
    }
}