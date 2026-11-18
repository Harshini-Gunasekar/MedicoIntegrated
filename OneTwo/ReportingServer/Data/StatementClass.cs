using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportingServer.Component.Pages;
using ReportingServer.Model;

namespace ReportingServer.Data
{
    public class StatementClass
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public StatementClass(
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<string> ReturnStatement(StatementRequest request)
        {
            using var scope = _scopeFactory.CreateScope();

            await using var htmlRenderer = new HtmlRenderer(
                scope.ServiceProvider,
                _loggerFactory);

            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<Statement>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["statements"] = request.statements,
                                ["LogoImage"] = request.LogoImage,
                                ["BranchName"] = request.BranchName,
                                ["CompanyName"] = request.CompanyName,
                                ["CompanyAddress"] = request.CompanyAddress,
                                ["CompanyContactNo"] = request.CompanyContactNo,
                                ["CompanyEmail"] = request.CompanyEmail,
                                ["fromdate"] = request.fromdate,
                                ["todate"] = request.todate
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
