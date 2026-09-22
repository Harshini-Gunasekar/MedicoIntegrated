using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class MasterTenantServices
    {
        private readonly HttpClient _http;

        public MasterTenantServices(HttpClient http)
        {
            _http = http;
        }

        private MultipartFormDataContent ToFormData(
            TenantReportMethodModel model, 
            bool isUpdate, 
            Dictionary<string, (byte[] Bytes, string FileName, string ContentType)>? files = null)
        {
            var content = new MultipartFormDataContent();

            if (isUpdate || model.id > 0)
            {
                content.Add(new StringContent(model.id.ToString()), "id");
            }

            content.Add(new StringContent(model.tenant_code ?? string.Empty), "tenant_code");
            if (model.logo_url != null)
                content.Add(new StringContent(model.logo_url), "logo_url");

            content.Add(new StringContent(model.isletterhead.ToString().ToLower()), "isletterhead");
            if (model.letterhead_url != null)
                content.Add(new StringContent(model.letterhead_url), "letterhead_url");

            content.Add(new StringContent(model.isletterbottom.ToString().ToLower()), "isletterbottom");
            if (model.bottom_url != null)
                content.Add(new StringContent(model.bottom_url), "bottom_url");

            content.Add(new StringContent(model.all_branch.ToString().ToLower()), "all_branch");

            content.Add(new StringContent(model.iswatermark.ToString().ToLower()), "iswatermark");
            if (model.watermark_url != null)
                content.Add(new StringContent(model.watermark_url), "watermark_url");

            content.Add(new StringContent(model.report_title ?? string.Empty), "report_title");
            content.Add(new StringContent(model.report_font ?? "Arial"), "report_font");
            content.Add(new StringContent((model.report_font_size ?? 12).ToString()), "report_font_size");
            content.Add(new StringContent(model.report_color ?? "#000000"), "report_color");

            content.Add(new StringContent(model.paper_size ?? "A4"), "paper_size");
            content.Add(new StringContent(model.orientation ?? "Portrait"), "orientation");

            content.Add(new StringContent(model.margin_top.ToString()), "margin_top");
            content.Add(new StringContent(model.margin_bottom.ToString()), "margin_bottom");
            content.Add(new StringContent(model.margin_left.ToString()), "margin_left");
            content.Add(new StringContent(model.margin_right.ToString()), "margin_right");

            content.Add(new StringContent(model.issignature.ToString().ToLower()), "issignature");
            if (model.signature_url != null)
                content.Add(new StringContent(model.signature_url), "signature_url");
            content.Add(new StringContent(model.signature_label ?? string.Empty), "signature_label");

            content.Add(new StringContent(model.isheadertext.ToString().ToLower()), "isheadertext");
            content.Add(new StringContent(model.header_text ?? string.Empty), "header_text");

            content.Add(new StringContent(model.isfootertext.ToString().ToLower()), "isfootertext");
            content.Add(new StringContent(model.footer_text ?? string.Empty), "footer_text");

            content.Add(new StringContent(model.isbarcode.ToString().ToLower()), "isbarcode");
            content.Add(new StringContent(model.isqrcode.ToString().ToLower()), "isqrcode");

            content.Add(new StringContent((model.show_printed_datetime ?? true).ToString().ToLower()), "show_printed_datetime");
            content.Add(new StringContent(model.datetime_format ?? "dd/MM/yyyy HH:mm"), "datetime_format");

            content.Add(new StringContent(model.deleted.ToString().ToLower()), "deleted");

            // Attach image files for backend controller [FromForm] IFormFile parameters
            if (files != null)
            {
                if (files.TryGetValue("logo", out var logoFile) && logoFile.Bytes?.Length > 0)
                {
                    var fileContent = new ByteArrayContent(logoFile.Bytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(logoFile.ContentType) ? "image/png" : logoFile.ContentType);
                    content.Add(fileContent, "logoFile", string.IsNullOrEmpty(logoFile.FileName) ? "logo.png" : logoFile.FileName);
                }

                if (files.TryGetValue("letterhead", out var lhFile) && lhFile.Bytes?.Length > 0)
                {
                    var fileContent = new ByteArrayContent(lhFile.Bytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(lhFile.ContentType) ? "image/png" : lhFile.ContentType);
                    content.Add(fileContent, "letterheadFile", string.IsNullOrEmpty(lhFile.FileName) ? "letterhead.png" : lhFile.FileName);
                }

                if (files.TryGetValue("bottom", out var bottomFile) && bottomFile.Bytes?.Length > 0)
                {
                    var fileContent = new ByteArrayContent(bottomFile.Bytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(bottomFile.ContentType) ? "image/png" : bottomFile.ContentType);
                    content.Add(fileContent, "bottomFile", string.IsNullOrEmpty(bottomFile.FileName) ? "bottom.png" : bottomFile.FileName);
                }

                if (files.TryGetValue("watermark", out var wmFile) && wmFile.Bytes?.Length > 0)
                {
                    var fileContent = new ByteArrayContent(wmFile.Bytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(wmFile.ContentType) ? "image/png" : wmFile.ContentType);
                    content.Add(fileContent, "watermarkFile", string.IsNullOrEmpty(wmFile.FileName) ? "watermark.png" : wmFile.FileName);
                }

                if (files.TryGetValue("signature", out var sigFile) && sigFile.Bytes?.Length > 0)
                {
                    var fileContent = new ByteArrayContent(sigFile.Bytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(sigFile.ContentType) ? "image/png" : sigFile.ContentType);
                    content.Add(fileContent, "signatureFile", string.IsNullOrEmpty(sigFile.FileName) ? "signature.png" : sigFile.FileName);
                }
            }

            return content;
        }

        public async Task<TenantReportMethodModel?> GetTenantReportMethodAsync(string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.GetAsync("api/TenantReportMethod/get");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<TenantReportMethodModel>(content);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tenant report method settings: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertTenantReportMethodAsync(
            TenantReportMethodModel model, 
            string tenantCode,
            Dictionary<string, (byte[] Bytes, string FileName, string ContentType)>? files = null)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                using var formData = ToFormData(model, isUpdate: false, files: files);
                var response = await _http.PostAsync("api/TenantReportMethod/insert", formData);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Insert error response: {response.StatusCode} - {err}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting tenant report method: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTenantReportMethodAsync(
            TenantReportMethodModel model, 
            string tenantCode,
            Dictionary<string, (byte[] Bytes, string FileName, string ContentType)>? files = null)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                using var formData = ToFormData(model, isUpdate: true, files: files);
                var response = await _http.PostAsync("api/TenantReportMethod/update", formData);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update error response: {response.StatusCode} - {err}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tenant report method: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTenantReportMethodAsync(int id, string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.GetAsync($"api/TenantReportMethod/delete?id={id}");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _http.DeleteAsync($"api/TenantReportMethod/delete?id={id}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tenant report method: {ex.Message}");
                return false;
            }
        }
    }
}
