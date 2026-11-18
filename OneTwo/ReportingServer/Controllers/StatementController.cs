using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatementController : ControllerBase
    {
        private readonly StatementClass _statement;
        public StatementController(StatementClass statement)
        {
            _statement = statement;
        }

        [HttpPost("GetdateStatement")]
        public async Task<IActionResult> GetStatement([FromBody] StatementRequest request)
        {
            var pdf = await _statement.ReturnStatement(request);
            return Ok(pdf);
        }
    }
}
