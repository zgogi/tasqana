using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Repositories
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
                .ToListAsync();
        }

        public async Task<Models.Todo?> GetByIdAsync(Models.User user, long id, bool asNoTracking)
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
