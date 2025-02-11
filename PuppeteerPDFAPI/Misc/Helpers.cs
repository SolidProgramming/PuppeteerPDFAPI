using PuppeteerSharp;
using System.IO;
using System.Runtime.InteropServices;

namespace PuppeteerPDFAPI.Misc
{
    public class Helpers
    {
        internal static string GetBrowserPath()
        {
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Path.Combine(Directory.GetCurrentDirectory(), "appdata", Globals.BinariesPath);

            string path = Path.Combine(Directory.GetCurrentDirectory(), Globals.BinariesPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
        internal static string? GetBrowserBinPath(string browserName)
        {
            string[] files;

            string path = Path.Combine(GetBrowserPath(), browserName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (browserName.Equals("chromium", StringComparison.CurrentCultureIgnoreCase))
                browserName = "chrome";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                browserName = $"{browserName}.exe";
            }

            files = Directory.GetFiles(path, browserName, new EnumerationOptions()
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = true,
            });

            return files.FirstOrDefault(_ => !_.EndsWith("dll"));
        }
    }
}