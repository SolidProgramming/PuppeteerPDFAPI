namespace PuppeteerPDFAPI.Models
{
    public class FileModel
    {
        public int Id { get; init; }
        public string? Guid { get; set; }
        public string? FileExtension { get; set; }
        public string Filename
        {
            get
            {
                return Guid + FileExtension;
            }
        }
        public string? Content { get; set; }
    }
}
