using Microsoft.AspNetCore.Mvc;
using ReportingServer.Data;
using ReportingServer.Model;

namespace ReportingServer.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BillReceiptController :ControllerBase
    {
        private readonly BillReceipt _bill;

        public BillReceiptController(BillReceipt bill)
        {
            _bill = bill;
        }

        [HttpPost("GetBill")]
        public async Task<IActionResult> GetBill([FromBody] CashBillModel bill)
        {

           var res= await _bill.ReturnBill(bill)?? new byte[0];


            return Ok(Convert.ToBase64String(res)??"");
        }
    }
}
