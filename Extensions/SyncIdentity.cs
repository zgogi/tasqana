using Microsoft.EntityFrameworkCore;
using Tasqana.Models;

namespace Tasqana.Extensions;

public static class SyncIdentity
{
    public static void Validate(Guid? syncId)
    {
        if (syncId == Guid.Empty) throw new InvalidRequestException("syncId must not be empty.");
    }

    public static IQueryable<T> WithIdentity<T>(this IQueryable<T> query, long? id, Guid? syncId)
        where T : class, ISyncEntity
    {
        Validate(syncId);
        if (id is not > 0 && syncId == null)
            throw new InvalidRequestException("id or syncId is required.");
        if (id is > 0) query = query.Where(x => x.Id == id.Value);
        if (syncId.HasValue) query = query.Where(x => x.SyncId == syncId.Value);
        return query;
    }

    public static async Task<T> RequireIdentityAsync<T>(this IQueryable<T> query, long? id, Guid? syncId)
        where T : class, ISyncEntity
        => await query.WithIdentity(id, syncId).SingleOrDefaultAsync() ?? throw new NotFoundException();

    public static T? FindIdentity<T>(this IEnumerable<T> items, long? id, Guid? syncId)
        where T : ISyncEntity
    {
        Validate(syncId);
        var byId = id is > 0 ? items.SingleOrDefault(x => x.Id == id) : default;
        var bySyncId = syncId.HasValue ? items.SingleOrDefault(x => x.SyncId == syncId) : default;
        var result = byId is not null ? byId : bySyncId;
        if (result != null && ((id is > 0 && result.Id != id) ||
            (syncId.HasValue && result.SyncId != syncId)))
            throw new InvalidRequestException("id and syncId refer to different records.");
        return result;
    }
}
