using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace LabCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        // Simple in-memory cache for scan results keyed by scanId
        private static readonly ConcurrentDictionary<string, ScanResult> _results = new();

        public class ScanResult
        {
            public string? Result { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }

        public class CallbackPayload
        {
            public string? Status { get; set; }
            public string? Message { get; set; }
            public string? Result { get; set; }
            public string? EntityId { get; set; }
            public string? EntityType { get; set; }
        }

        [HttpPost("callback/{scanId}")]
        public IActionResult Callback(string scanId, [FromBody] CallbackPayload payload)
        {
            if (payload != null && !string.IsNullOrEmpty(payload.Result))
            {
                _results[scanId] = new ScanResult { Result = payload.Result };
                return Ok(new { message = "Result received" });
            }
            return BadRequest(new { message = "Invalid payload" });
        }

        [HttpGet("status/{scanId}")]
        public IActionResult GetStatus(string scanId)
        {
            // Clean up old results occasionally (simple logic)
            var oldKeys = _results.Where(x => (DateTime.Now - x.Value.Timestamp).TotalMinutes > 5).Select(x => x.Key);
            foreach (var key in oldKeys) _results.TryRemove(key, out _);

            if (_results.TryRemove(scanId, out var result))
            {
                return Ok(new { success = true, path = result.Result });
            }
            return Ok(new { success = false });
        }
    }
}
