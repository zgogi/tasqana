namespace Tasqana.Models
{
    public class TodoFile : AbstractModel<TodoFile>, IOrderable
    {
        public long TodoId { get; set; }
        public int Order { get; set; }
        public string? Title { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = null!;
        public string? PreviewFileName { get; set; }
        public long? PreviewFileSize { get; set; }
        public string? PreviewContentType { get; set; }
        public Todo Todo { get; set; } = null!;
    }
}
