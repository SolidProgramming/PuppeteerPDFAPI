using PuppeteerPDFAPI.Misc;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace PuppeteerPDFAPI.Services
{
    public interface IPuppeteerService
    {
        Task InitializeAsync();
        Task<(Stream?, string? ErrorMessage)> ConvertAsync(string html);
    }

    public class PuppeteerService(ILogger<PuppeteerService> logger, IHostApplicationLifetime appLifetime) : IPuppeteerService
    {
        const SupportedBrowser TargetBrowser = SupportedBrowser.Chrome;
        IBrowser? Browser = default;

        public async Task InitializeAsync()
        {
            appLifetime.ApplicationStopping.Register(async () =>
            {
                await Shutdown();
            });

            string browserPath = Helpers.GetBrowserPath();

            BrowserFetcherOptions browserFetcherOptions = new() { Browser = TargetBrowser, Path = browserPath };
            BrowserFetcher? browserFetcher = new(browserFetcherOptions);

            InstalledBrowser? installedBrowser = browserFetcher.GetInstalledBrowsers()
                      .Where(_ => _.Browser == TargetBrowser).MaxBy(_ => _.BuildId);

            if (installedBrowser == null)
            {
                logger.LogWarning($"{DateTime.Now} [{nameof(PuppeteerService)}] | {TargetBrowser} is not installed! Trying to download latest version...");

                installedBrowser = await browserFetcher.DownloadAsync(BrowserTag.Latest);

                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {TargetBrowser} finished downloading...");
            }

            try
            {
                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | Trying to startup {TargetBrowser}...");

                Browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions()
                    {
                        ExecutablePath = Helpers.GetBrowserBinPath(TargetBrowser.ToString())
                    });

                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {TargetBrowser} startup finished.");
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {TargetBrowser} startup failed with error: {ex}.");
                throw;
            }
        }

        public async Task<(Stream?, string? ErrorMessage)> ConvertAsync(string html)
        {
            if (Browser is null)
                return (default, "Browser not initialized!");

            try
            {
                using IPage page = await Browser.NewPageAsync();
                await page.SetContentAsync(html);

                Stream pdfStream = await page.PdfStreamAsync();

                await page.CloseAsync();

                return (pdfStream, default);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now} [{nameof(PuppeteerService)}] | {ex}");
                return (null, ex.ToString());
            }
        }

        public async Task Shutdown()
        {
            if (Browser is null)
                return;

            await Browser.CloseAsync();
            await Browser.DisposeAsync();

        }
    }
}
