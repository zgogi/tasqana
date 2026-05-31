

using Microsoft.EntityFrameworkCore;

namespace WebApi.Repositories
{
    public class TelegramRepository : AbstractRepository<Models.TelegramMessage>
    {
        public TelegramRepository(TaskanaDb context)
            : base(context) { }


        public override async Task<List<Models.TelegramMessage>> GetAllAsync(bool asNoTracking=true)
        {
            var entities = await Query(asNoTracking)
                .Include(c => c.User)
                .OrderDescending()
                .ToListAsync();

            return entities;
        }



    }
}
