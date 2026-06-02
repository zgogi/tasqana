using Microsoft.EntityFrameworkCore;

namespace Tasqana.Repositories
{
    public class UsersRepository : AbstractRepository<Models.User>
    {
        public UsersRepository(TaskanaDb context)
            : base(context) { }

        public async Task<Models.User?> GetByTelegramIdAsync(long telegramId, bool asNoTracking = true)
        {
            return await Query(asNoTracking)
                .Where(c => c.TelegramId == telegramId)
                .SingleOrDefaultAsync();
        }
    }
}
