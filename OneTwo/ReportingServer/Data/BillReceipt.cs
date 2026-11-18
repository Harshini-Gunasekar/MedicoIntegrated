using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportingServer.Component.Pages;

using ReportingServer.Model;


namespace ReportingServer.Data
{
    public class BillReceipt
    {
        private readonly HtmlRenderer _htmlRenderer;

        public BillReceipt(
            IServiceProvider services,
            ILoggerFactory loggerFactory)
        {
            _htmlRenderer =
                new HtmlRenderer(
                    services,
                    loggerFactory);
        }

        // RENDER RAZOR COMPONENT

        public async Task<string> RenderComponentAsync<T>(
            Dictionary<string, object> parameters = null)
            where T : IComponent
        {
            parameters ??= new();

            var parameterView =
                ParameterView.FromDictionary(parameters);

            var output =
                await _htmlRenderer.Dispatcher.InvokeAsync(
                async () =>
                {
                    var rendered =
                        await _htmlRenderer.RenderComponentAsync<T>(
                            parameterView);

                    return rendered.ToHtmlString();
                });

            return output;
        }

        // PDF

        public async Task<byte[]> ReturnBill(CashBillModel bill)
        {
            // DYNAMIC ROUTING FOR "Modern" TENANT
            if (bill.TenantId == "Modern")
            {
                string designsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ReportDesigns");
                if (!Directory.Exists(designsPath))
                {
                    designsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "ReportDesigns");
                }

                string headerPath = Path.Combine(designsPath, "ModernInvoiceHeader.html");
                string bodyPath = Path.Combine(designsPath, "Modernbillinvoice.html");
                string footerPath = Path.Combine(designsPath, "ModernInvoiceFooter.html");

                if (File.Exists(headerPath) && File.Exists(bodyPath) && File.Exists(footerPath))
                {
                    string rawHeader = await File.ReadAllTextAsync(headerPath);
                    string rawBody = await File.ReadAllTextAsync(bodyPath);
                    string rawFooter = await File.ReadAllTextAsync(footerPath);

                    Func<string, string> escapeHtml = (val) => System.Web.HttpUtility.HtmlEncode(val ?? "");

                    string billDateStr = bill.BillDate.ToString("dd-MM-yyyy");
                    string billDateShortStr = bill.BillDate.ToString("dd/MM/yy");
                    string createdTimeStr = bill.CreatedTime.ToString("hh:mmtt").ToUpper();

                    var itemsRowsList = new System.Text.StringBuilder();
                    var itemsSlipRowsList = new System.Text.StringBuilder();
                    for (int i = 0; i < bill.Items.Count; i++)
                    {
                        var item = bill.Items[i];
                        itemsRowsList.Append($@"
                        <tr>
                            <td style='width: 12%; text-align: left;'>{i + 1}</td>
                            <td style='width: 68%; text-align: left; text-transform: uppercase;'>{escapeHtml(item.TestName)}</td>
                            <td style='width: 20%; text-align: right;'>{item.Amount.ToString("F2")}</td>
                        </tr>");

                        itemsSlipRowsList.Append($@"
                        <tr>
                            <td style='width: 12%; text-align: left; vertical-align: top;'>{i + 1}</td>
                            <td style='width: 88%; text-align: left; text-transform: uppercase; vertical-align: top;'>{escapeHtml(item.TestName)}</td>
                        </tr>");
                    }

                    string headerHtml = rawHeader
                        .Replace("{{LabName}}", escapeHtml(bill.LabName))
                        .Replace("{{Address}}", escapeHtml(bill.Address).Replace("\n", "<br/>"))
                        .Replace("{{MobileNo}}", escapeHtml(bill.MobileNo))
                        .Replace("{{HelplineNo}}", escapeHtml(bill.HelplineNo))
                        .Replace("{{BillNo}}", escapeHtml(bill.BillNo))
                        .Replace("{{BillDate}}", billDateStr)
                        .Replace("{{BillDateShort}}", billDateShortStr)
                        .Replace("{{PatientName}}", escapeHtml(bill.PatientName))
                        .Replace("{{Age}}", escapeHtml(bill.Age))
                        .Replace("{{Gender}}", escapeHtml(bill.Gender))
                        .Replace("{{PatientAddress}}", escapeHtml(bill.PatientAddress))
                        .Replace("{{DoctorName}}", escapeHtml(bill.DoctorName))
                        .Replace("{{OnlineCode}}", escapeHtml(bill.OnlineCode))
                        .Replace("{{Password}}", escapeHtml(bill.Password));

                    string bodyHtml = rawBody
                        .Replace("{{ItemsRows}}", itemsRowsList.ToString())
                        .Replace("{{ItemsSlipRows}}", itemsSlipRowsList.ToString())
                        .Replace("{{CreatedBy}}", escapeHtml(bill.CreatedBy))
                        .Replace("{{CreatedTime}}", createdTimeStr)
                        .Replace("{{TotalAmount}}", bill.TotalAmount.ToString("F2"))
                        .Replace("{{DiscountAmount}}", bill.DiscountAmount.ToString("F2"))
                        .Replace("{{ReceivedAmount}}", bill.ReceivedAmount.ToString("F2"))
                        .Replace("{{BalanceAmount}}", bill.BalanceAmount.ToString("F2"));

                    string footerHtml = rawFooter;

                    return await RenderPdfFromHtml(headerHtml, bodyHtml, footerHtml);
                }
            }

            // STANDARD BLACOR COMPONENTS FLOW FOR OTHER TENANTS
            string standardHeaderHtml =
                await RenderComponentAsync<InvoiceHeader>(
                new Dictionary<string, object>
                {
                    { "bills", bill }
                });

            string standardBodyHtml =
                await RenderComponentAsync<InvoiceBody>(
                new Dictionary<string, object>
                {
                    { "Bills", bill },
                    { "Items", bill.Items }
                });

            string standardFooterHtml =
                await RenderComponentAsync<InvoiceFooter>();

            return await RenderPdfFromHtml(standardHeaderHtml, standardBodyHtml, standardFooterHtml);
        }

        private async Task<byte[]> RenderPdfFromHtml(string headerHtml, string bodyHtml, string footerHtml)
        {
            // DOWNLOAD CHROMIUM
            await new BrowserFetcher().DownloadAsync();

            // BROWSER
            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(bodyHtml);
            await page.EmulateMediaTypeAsync(MediaType.Print);

            // PDF PRINTING
            byte[] pdf = await page.PdfDataAsync(
                new PdfOptions
                {
                    Format = PaperFormat.A5,
                    Landscape = false,
                    PrintBackground = true,
                    PreferCSSPageSize = true,
                    DisplayHeaderFooter = true,
                    HeaderTemplate = $@"
<div style='
    width:100%;
    padding:1px;
    box-sizing:border-box;
    font-family:Arial;
    background:white;
    '>
    {headerHtml}
</div>",
                    FooterTemplate = $@"
<div style='
    width:100%;
    padding:1px;
    box-sizing:border-box;
    font-family:Arial;
    font-size:10px;
    background:white;
    '>
    {footerHtml}
    <div style='text-align:center;'>
        Page <span class='pageNumber'></span> of <span class='totalPages'></span>
    </div>
</div>",
                    MarginOptions = new MarginOptions
                    {
                        Top = "240px",
                        Bottom = "70px",
                        Left = "10px",
                        Right = "10px"
                    }
                });

            await browser.CloseAsync();
            return pdf;
        }
    }
}