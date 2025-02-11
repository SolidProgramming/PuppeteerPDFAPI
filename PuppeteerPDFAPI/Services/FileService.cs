using HtmlAgilityPack;
using PuppeteerSharp;
using System.IO;
using System.Text;

namespace PuppeteerPDFAPI.Services
{
    public interface IFileService
    {
        Task<(FileModel?, string? ErrorMessage)> ProcessAsync(IFormFile file);
    }

    public class FileService(ILogger<FileService> logger) : IFileService
    {
        public async Task<(FileModel?, string? ErrorMessage)> ProcessAsync(IFormFile formFile)
        {
            string? extension = Path.GetExtension(formFile.FileName);

            if (extension == null)
                return (default, "No file extension found!");

            if (!extension.Equals(".html"))
                return (default, "File has to be of type HTML!");

            using StreamReader sr = new(formFile.OpenReadStream(), Encoding.UTF8);

            string content = await sr.ReadToEndAsync();

            if (!IsValidHtml(content))
                return (default, "Invalid HTML!");

            FileModel file = new()
            {
                Guid = Guid.NewGuid().ToString(),
                FileExtension = extension,
                Content = content,
            };

            return (file, default);
        }

        private bool IsValidHtml(string html)
        {
            try
            {
                HtmlDocument? doc = new();
                doc.LoadHtml(html);
                return !doc.ParseErrors.Any();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now} [{nameof(FileService)}] | {ex}");
                return false;
            }
        }
    }
}
