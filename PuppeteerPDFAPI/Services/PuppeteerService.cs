using PuppeteerPDFAPI.Misc;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using System.IO;
using System.Reflection;

namespace PuppeteerPDFAPI.Services
{
    public interface IPuppeteerService
    {
        Task InitializeAsync();
    }

    public class PuppeteerService(ILogger<PuppeteerService> logger) : IPuppeteerService
    {
        const SupportedBrowser Browser = SupportedBrowser.Chrome;

        public async Task InitializeAsync()
        {
            string browserPath = Helpers.GetBrowserPath();

            BrowserFetcherOptions browserFetcherOptions = new() { Browser = Browser, Path = browserPath };
            BrowserFetcher? browserFetcher = new(browserFetcherOptions);

            InstalledBrowser? installedBrowser = browserFetcher.GetInstalledBrowsers()
                      .Where(_ => _.Browser == Browser).MaxBy(_ => _.BuildId);

            if (installedBrowser == null)
            {
                logger.LogWarning($"{DateTime.Now} [{nameof(PuppeteerService)}] | {Browser} is not installed! Trying to download latest version...");

                installedBrowser = await browserFetcher.DownloadAsync(BrowserTag.Latest);

                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {Browser} finished downloading...");
            }

            try
            {
                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | Trying to startup {Browser}...");
                using IBrowser? browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions()
                    {
                        ExecutablePath = Helpers.GetBrowserBinPath(Browser.ToString())
                    });

                await browser.NewPageAsync();
                await browser.CloseAsync();

                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {Browser} startup finished.");
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{DateTime.Now} [{nameof(PuppeteerService)}] | {Browser} startup failed with error: {ex}.");
                throw;
            }
        }
    }
}
