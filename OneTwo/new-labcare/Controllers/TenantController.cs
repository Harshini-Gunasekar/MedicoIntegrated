using LabCare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LabCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : BaseApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        public TenantController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var client = _httpClientFactory.CreateClient("LabCareApi");
            var response = await client.PostAsJsonAsync("Tenant/forgotpassword", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ForgetResponseModel>();
                return Ok(result);
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var client = _httpClientFactory.CreateClient("LabCareApi");
            var response = await client.PostAsJsonAsync("Tenant/resetpassword", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(new { message = result });
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpGet("sso-login")]
        [HttpGet("ssologin")]
        public IActionResult SsoLoginGet() => Ok(new { message = "SSO GET endpoint active", status = "success" });

        [HttpPost("sso-login")]
        [HttpPost("ssologin")]
        public IActionResult SsoLoginPost([FromBody] object? details = null)
        {
            return Ok(new { message = "SSO POST details received", status = "success" });
        }
    }
}
