using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CultureController : ControllerBase
    {
        private readonly CultureClass _culture;

        public CultureController(CultureClass culture)
        {
            _culture = culture;
        }

        [HttpPost("GetCultureReport")]
        public async Task<IActionResult> GetCultureReport([FromBody] CultureReportDto dto)
        {
            var pdf = await _culture.ReturnCultureReport(dto) ?? new byte[0];
            return Ok(Convert.ToBase64String(pdf) ?? "");
        }
    }
}
