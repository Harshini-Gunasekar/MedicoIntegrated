using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace LabCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        // HTTP port — iscan_app.exe fetches over plain HTTP to avoid
        // self-signed TLS certificate rejection in the development environment.
        // This matches the "http" profile in launchSettings.json.
        private const string HttpBaseUrl = "http://localhost:5155";

        private static readonly ConcurrentDictionary<string, CachedPdf> _pdfCache = new();

        private class CachedPdf
        {
            public string Base64 { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        public class StoreRequest
        {
            public string Base64 { get; set; } = string.Empty;
        }

        /// <summary>
        /// Caches a base64-encoded PDF in memory and returns the full HTTP URL
        /// that iscan_app.exe can call to retrieve the raw PDF bytes.
        /// Using HTTP (port 5155) avoids self-signed TLS cert issues in the desktop app.
        /// </summary>
        public static string StorePdf(string base64)
        {
            // Clean up old entries (older than 5 minutes) to prevent memory leaks
            var now = DateTime.UtcNow;
            var expiredKeys = _pdfCache.Where(x => (now - x.Value.CreatedAt).TotalMinutes > 5)
                                       .Select(x => x.Key).ToList();
            foreach (var key in expiredKeys)
                _pdfCache.TryRemove(key, out _);

            var id = Guid.NewGuid().ToString("N");
            _pdfCache[id] = new CachedPdf { Base64 = base64, CreatedAt = now };

            // Return the full HTTP URL — the JS will btoa() this and fire labcareprint://
            return $"{HttpBaseUrl}/api/print/getbill?id={id}";
        }

        [HttpPost("store")]
        public IActionResult StorePost([FromBody] StoreRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Base64))
                return BadRequest("Empty base64 data.");

            var url = StorePdf(request.Base64);
            return Ok(new { url });
        }

        [HttpGet("getbill")]
        public IActionResult GetBill([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Missing id parameter.");

            // Use TryGetValue instead of TryRemove so that the PDF can be fetched
            // multiple times (e.g. for preview + print, or retries) without 404 errors.
            // Old entries are automatically cleaned up after 5 minutes in StorePdf().
            if (_pdfCache.TryGetValue(id, out var cached))
            {
                try
                {
                    var base64Data = cached.Base64.Trim();
                    if (base64Data.StartsWith("data:application/pdf;base64,", System.StringComparison.OrdinalIgnoreCase))
                        base64Data = base64Data["data:application/pdf;base64,".Length..];

                    var pdfBytes = System.Convert.FromBase64String(base64Data);

                    // Allow the desktop app to fetch this even from a different origin
                    Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    Response.Headers.Append("Access-Control-Allow-Methods", "GET");

                    // Return raw binary PDF bytes — iscan_app.exe reads these directly
                    return File(pdfBytes, "application/pdf");
                }
                catch (System.Exception ex)
                {
                    return BadRequest($"Failed to decode cached PDF: {ex.Message}");
                }
            }

            return NotFound("PDF receipt not found or expired.");
        }
    }
}
