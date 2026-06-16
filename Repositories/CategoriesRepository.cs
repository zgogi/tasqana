using Microsoft.EntityFrameworkCore;
using Tasqana.Extensions;
using Tasqana.Models;

namespace Tasqana.Repositories
{
    public class CategoriesRepository : AbstractRepository<Models.Category>
    {
        public CategoriesRepository(TaskanaDb context)
            : base(context) { }

        public async Task<List<Models.Category>> GetByUserAndParentAsync(Models.User user, long? parentId, bool asNoTracking)
        {
            return await Query(asNoTracking)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Order)
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
            var item = await GetByIdAsync(user, id, false);
            if (item == null) throw new NotFoundException();
            await RemoveAsync(item);

        }

    }
}
