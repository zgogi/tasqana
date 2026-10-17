using Tasqana.Extensions;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Tasqana.Models;
using Tasqana.Repositories;

namespace Tasqana.Repositories
{
    public class CheckItemRepository : AbstractRepository<CheckItem>
    {
        public CheckItemRepository(TaskanaDb context):base(context) { }

        public async Task<Models.CheckItem?> GetByIdAsync(Models.User user, long id, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(e => e.Id == id && e.Todo.UserId == user.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<List<Models.CheckItem>> GetByParentIdAsync(Models.Todo todo, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(e => e.TodoId == todo.Id)
                .ToListAsync();
        }

        public Task<Models.CheckItem> ResolveAsync(Models.User user, long? id, Guid? syncId, bool asNoTracking = false)
        {
            return Query(asNoTracking).Where(c => c.Todo.UserId == user.Id)
                .RequireIdentityAsync(id, syncId);
        }

    }
}
