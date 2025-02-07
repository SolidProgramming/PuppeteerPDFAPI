using HtmlAgilityPack;
using PuppeteerPDFAPI.Models;

namespace PuppeteerPDFAPI.Services
{
    public interface IFileService
    {
        Task<(bool Success, string? ErrorMessage)> UploadAsync(FileModel file);
        bool IsValidHtml();
    }

    public class FileService(ILogger<FileService> logger) : IFileService
    {
        public bool IsValidHtml()
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, string? ErrorMessage)> UploadAsync(FileModel file)
        {
            return await Task.Run<(bool, string?)>(() =>
            {
                try
                {
                    using Stream stream = new FileStream(file.Guid + file.FileExtension, FileMode.Create);
                    file.File.CopyTo(stream);
                    return (true, default);
                }
                catch (Exception ex)
                {
                    return (false, ex.ToString());
                }
            });
        }

        private static bool IsValidHTML(string html)
        {
            try
            {
                HtmlDocument? doc = new();
                doc.LoadHtml(html);
                return !doc.ParseErrors.Any();
            }
            catch (Exception)
            {
                return false;
            }

        }

    }
}
