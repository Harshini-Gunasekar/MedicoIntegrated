using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportingServer.Component.Pages;
using ReportingServer.Model;
using System.Text;

namespace ReportingServer.Data
{
    public class RoutineReportClass
    {

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;
        
        public RoutineReportClass(IServiceScopeFactory scopeFactory,  ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
            
        }
        public async Task<string> RenderComponentAsync<T>(
    Dictionary<string, object> parameters = null)
    where T : IComponent
        {
            parameters ??= new();
            using var scope = _scopeFactory.CreateScope();
            await using var _htmlRenderer =
                new HtmlRenderer(
                    scope.ServiceProvider,
                    _loggerFactory);
            return await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var rendered = await _htmlRenderer.RenderComponentAsync<T>(
                    ParameterView.FromDictionary(parameters));

                return rendered.ToHtmlString();
            });
        }
        public async Task<string> ReturnRoutineReport(RoutineReportRequest request)
        {
            var rpt = request.results;

            using var scope = _scopeFactory.CreateScope();

            await using var htmlRenderer = new HtmlRenderer(
                scope.ServiceProvider,
                _loggerFactory);

            // Render Razor Component to HTML
            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<RoutineReport>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["rrm"] = rpt,
                                ["HeaderImage"] = request.HeaderImage,
                                ["IsLetterhead"] = request.IsLetterhead
                            }));

                return renderedComponent.ToHtmlString();
            });

            // Build footer image block from request wrapper
            string footerImageBlock;
            if (request.FooterImage != null && request.FooterImage.Length > 0 && !request.IsLetterhead)
            {
                string base64Footer = Convert.ToBase64String(request.FooterImage);
                footerImageBlock = $"<img src='data:image/png;base64,{base64Footer}' style='width:100%; display:block;' />";
            }
            else
            {
                // Blank spacer for pre-printed letterhead
                footerImageBlock = "<div style='height:30px; width:100%;'></div>";
            }

            // Launch Chromium
            await new BrowserFetcher().DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            });

            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);
            await page.WaitForSelectorAsync(".tb");
            await page.WaitForNetworkIdleAsync();
            await page.EmulateMediaTypeAsync(MediaType.Print);
            await Task.Delay(500);

            // Generate PDF with footer pinned to page bottom
            byte[] pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                DisplayHeaderFooter = true,
                HeaderTemplate = "<div></div>",
                FooterTemplate = $@"
<div style='
    width:100%;
    padding:0;
    margin:0;
    box-sizing:border-box;
    background:white;
'>
    {footerImageBlock}
    <div style='text-align:center; font-family:Arial; font-size:9px; color:#718096; padding:2px 0;'>
        Page <span class='pageNumber'></span> of <span class='totalPages'></span>
    </div>
</div>",
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Bottom = "70px",
                    Left = "10mm",
                    Right = "10mm"
                }
            });

            return Convert.ToBase64String(pdfBytes);
        }
        

    }
}
