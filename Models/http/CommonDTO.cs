namespace Tasqana.Models.http
{
    public class DeleteDTO
    {
        public long id { get; set; }
        public Guid? syncId { get; set; }
    }

    public class ReorderDTO
    {
        public long id { get; set; }
        public Guid? syncId { get; set; }
        public long? before_id { get; set; }
        public Guid? beforeSyncId { get; set; }

    }
}
