//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Repositories
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

       // public async Task<List<Models.CheckItem>> GetByParentIdAsync(Models.User user, long todoId, bool asNoTracking)
       // {

       // }

    }
}
