using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutineReportController : ControllerBase
    {
        private readonly RoutineReportClass rrc;
        HttpClient http;
        public RoutineReportController(RoutineReportClass _rrc, HttpClient http)
        {
            rrc = _rrc;
            this.http = http;
        }

        [HttpPost("LabReport")]
        public async Task<IActionResult> GetRoutineReport([FromBody] RoutineReportRequest request)
        {

            var res1 = await rrc.ReturnRoutineReport(request);
            
           
            return Ok(res1);
        }
    }
}
