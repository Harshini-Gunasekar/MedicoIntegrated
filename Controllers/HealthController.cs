using System.Threading;
using System.Threading.Tasks;
using Booking.Models;
using Booking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthService _healthService;

        public HealthController(HealthService healthService)
        {
            _healthService = healthService;
        }

        [HttpGet("Check")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Check(CancellationToken cancellationToken)
        {
            var result = await _healthService.CheckHealthAsync(cancellationToken);
            return Ok(result);
        }
    }
}
