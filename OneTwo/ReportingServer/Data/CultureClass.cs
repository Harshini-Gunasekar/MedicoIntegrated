using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportingServer.Component.Pages;
using ReportingServer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportingServer.Data
{
    public class CultureClass
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public CultureClass(
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<byte[]> ReturnCultureReport(CultureReportDto dto)
        {
            using var scope = _scopeFactory.CreateScope();

            await using var htmlRenderer = new HtmlRenderer(
                scope.ServiceProvider,
                _loggerFactory);

            // 1. Render Header component to HTML
            string headerHtml = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<Culture>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["dto"] = dto,
                                ["Section"] = "Header"
                            }));

                return renderedComponent.ToHtmlString();
            });

            // 2. Render Footer component to HTML
            string footerHtml = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<Culture>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["dto"] = dto,
                                ["Section"] = "Footer"
                            }));

                return renderedComponent.ToHtmlString();
            });

            // 3. Render Body component to HTML
            string bodyHtml = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var renderedComponent =
                    await htmlRenderer.RenderComponentAsync<Culture>(
                        ParameterView.FromDictionary(
                            new Dictionary<string, object?>
                            {
                                ["dto"] = dto,
                                ["Section"] = "Body"
                            }));

                return renderedComponent.ToHtmlString();
            });

            return await RenderPdfFromHtml(headerHtml, bodyHtml, footerHtml);
        }

        private async Task<byte[]> RenderPdfFromHtml(string headerHtml, string bodyHtml, string footerHtml)
        {
            await new BrowserFetcher().DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(bodyHtml);
            await page.EmulateMediaTypeAsync(MediaType.Print);

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
    <div style='text-align:center; font-family: ""Segoe UI"", Arial; font-size: 9px; color: #718096; margin-top: 2px; padding-bottom: 2px;'>
        Page <span class='pageNumber'></span> of <span class='totalPages'></span>
    </div>
</div>",
                    MarginOptions = new MarginOptions
                    {
                        Top = "265px",
                        Bottom = "108px",
                        Left = "20px",
                        Right = "20px"
                    }
                });

            await browser.CloseAsync();
            return pdf;
        }
    }
}
