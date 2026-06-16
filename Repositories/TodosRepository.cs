using Microsoft.EntityFrameworkCore;
using Tasqana.Extensions;
using Tasqana.Models;
using Tasqana.Models.http;

namespace Tasqana.Repositories
{
    public class TodosRepository : AbstractRepository<Models.Todo>
    {
        public TodosRepository(TaskanaDb context)
            : base(context) { }


        public override async Task<List<Models.Todo>> GetAllAsync(bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Include(c => c.User)
                .Include(c => c.Category)
                .OrderBy(c => c.CategoryId)
                .ToListAsync();
        }

        public async Task<List<Models.Todo>> GetByCategoryAsync(Models.User user, long? categoryId, bool asNoTracking = true)
        {
            var query = Query(asNoTracking)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.CategoryId == categoryId)
                .Where(c => c.State != TodoState.Completed)
                .Include(c => c.CheckItems.OrderBy(e => e.Order))
                .Include(c => c.Media.OrderBy(e => e.Order))
                .OrderBy(e => e.Order);
            return await query.ToListAsync();
        }

        public async Task<List<Models.Todo>> GetByPriorityAsync(Models.User user, Priority minPriority)
        {
            var query = Query(true)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Priority >= minPriority)
                .Where(c => c.State != TodoState.Completed)
                .Include(c => c.CheckItems.OrderBy(e => e.Order))
                .Include(c => c.Media.OrderBy(e => e.Order))
                .OrderByDescending(c => c.Priority);
            return await query.ToListAsync();
        }

        public async Task<List<Models.Todo>> GetByStateAsync(Models.User user, TodoState state)
        {
            var query = Query(true)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.State == state)
                .Include(c => c.CheckItems.OrderBy(e => e.Order))
                .Include(c => c.Media.OrderBy(e => e.Order))
                .OrderByDescending(e => e.UpdatedAt);
            return await query.ToListAsync();
        }


        public async Task<Models.Todo?> GetByIdAsync(Models.User user, long id, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(c => c.UserId == user.Id && c.Id == id)
                .Include(c => c.CheckItems.OrderBy(e => e.Order))
                .Include(c => c.Media.OrderBy(e => e.Order))
                .SingleOrDefaultAsync();
        }

        public async Task DeleteAsync(User user, long id)
        {
            var item = await GetByIdAsync(user, id, false);
            if (item == null) throw new NotFoundException();
            await RemoveAsync(item);
        }

    }
}
