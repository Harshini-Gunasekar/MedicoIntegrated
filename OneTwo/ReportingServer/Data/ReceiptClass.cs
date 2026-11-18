using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportingServer.Component.Pages;
using ReportingServer.Model;

namespace ReportingServer.Data
{
    public class ReceiptClass
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ReceiptClass(
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<string> ReturnReceipt(ReceiptRequest request)
        {
            using var scope = _scopeFactory.CreateScope();

            await using var htmlRenderer = new HtmlRenderer(
                scope.ServiceProvider,
                _loggerFactory);

            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<Receipt>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["ReceiptData"] = request.ReceiptData,
                                ["LogoImage"] = request.LogoImage
                            }));

                return renderedComponent.ToHtmlString();
            });

            await new BrowserFetcher().DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });

            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            await page.EmulateMediaTypeAsync(MediaType.Print);

            byte[] pdfBytes = await page.PdfDataAsync(
                new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                });

            return Convert.ToBase64String(pdfBytes);
        }
    }
}