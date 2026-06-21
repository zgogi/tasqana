using Tasqana.Services;

namespace Tasqana.Models.http
{
    public class FileDTO
    {
        public long? id { get; set; }
        public string? url { get; set; }
        public string? mime { get; set; }
        public string? title { get; set; }
        public bool is_deleted { get; set; }
        public IFormFile? content { get; set; }

        public FileDTO() { }

        public FileDTO(Models.TodoFile source, Func<string, string> fileToUrl)
        {
            id = source.Id;
            url = fileToUrl(source.FileName);
            mime = source.ContentType;
            title = source.Title;
        }
    }

    

    
}
