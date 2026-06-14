namespace Tasqana.Models
{
    public class TodoMedia : AbstractModel<TodoMedia>, IOrderable
    {
        public long TodoId { get; set; }
        public int Order { get; set; }
        public string? Title { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }

        public Todo Todo { get; set; } = null!;
    }
}
