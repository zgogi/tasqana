namespace Tasqana.Models;

public interface ISyncEntity
{
    long Id { get; set; }
    Guid SyncId { get; set; }
}
