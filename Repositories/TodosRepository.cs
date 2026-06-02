using Microsoft.EntityFrameworkCore;
using Tasqana.Models;
using Tasqana.Models.http;

namespace Tasqana.Repositories
{
    public class TodosRepository : AbstractRepository<Models.Todo>
    {
        public TodosRepository(TaskanaDb context)
            : base(context) { }


        public async Task<List<Models.Todo>> GetByCategoryAsync(Models.User user, long? categoryId)
        {
            return await Query(true)
                .Where(c => c.UserId == user.Id 
                         && c.CategoryId == categoryId 
                         && c.State != TodoState.Completed)
                .Include(c => c.CheckItems.OrderBy(e=>e.Order))
                .OrderBy(e => e.Order)
                .ToListAsync();
        }

        public async Task<Models.Todo?> GetByIdAsync(Models.User user, long id, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(c => c.UserId == user.Id && c.Id == id)
                .Include(c => c.CheckItems.OrderBy(e => e.Order))
                .SingleOrDefaultAsync();
        }

        public async Task DeleteAsync(User user, long id)
        {
            await QueryById(id, false)
                .Where(c => c.UserId == user.Id)
                .ExecuteDeleteAsync();

        }

    }
}
