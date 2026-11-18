using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly ReceiptClass _receipt;
        public ReceiptController(ReceiptClass receipt)
        {
            _receipt = receipt;
        }

        [HttpPost("GetReceipt")]
        public async Task<IActionResult> GetReceipt([FromBody] ReceiptRequest dto)
        {
            var pdf = await _receipt.ReturnReceipt(dto);
            return Ok(pdf);
        }
    }
}
