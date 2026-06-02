using Microsoft.EntityFrameworkCore;
using Tasqana.Models;

namespace Tasqana.Repositories
{
    public class CategoriesRepository : AbstractRepository<Models.Category>
    {
        public CategoriesRepository(TaskanaDb context)
            : base(context) { }

        public async Task<List<Models.Category>> GetByUserAndParent(Models.User user, long? parentId)
        {
            return await Query(true)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.ParentId == parentId)
                .ToListAsync();
        }

        public async Task<List<Models.Category>> GetByUser(Models.User user)
        {
            return await Query(true)
                .Where(c => c.UserId == user.Id)
                .Include(e => e.Todos)
                .ToListAsync();
        }

        public async Task<Models.Category?> GetByIdAsync(Models.User user, long id, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Id == id)
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
