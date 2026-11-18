using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly SummaryClass _cls;
        public SummaryController(SummaryClass cls)
        {
            _cls = cls;
        }

        [HttpPost("GetdateSummary")]
        public async Task<IActionResult> GetSummary([FromBody] SummaryRequest request)
        {
            var pdf = await _cls.ReturnSummary(request);
            return Ok(pdf);
        }
    }
}
