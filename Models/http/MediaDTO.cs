namespace Tasqana.Models.http
{
    public class MediaDTO
    {
        public long id { get; set; }
        public string url { get; set; } = null!;
        public string mime { get; set; } = null!;
        public string? title { get; set; }
        
        public MediaDTO(Models.TodoMedia source, Func<string, string> fileToUrl)
        {
            id = source.Id;
            url = fileToUrl(source.FileName);
            mime = source.MimeType ?? "";
            title = source.Title;
        }
    }

    
}
