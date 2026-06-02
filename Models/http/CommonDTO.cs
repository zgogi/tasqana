namespace WebApi.Models.http
{
    public class DeleteDTO
    {
        public long id { get; set; }
    }

    public class ReorderDTO
    {
        public long id { get; set; }
        public long? before_id { get; set; }

    }
}
