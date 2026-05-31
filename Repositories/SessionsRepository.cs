using Microsoft.EntityFrameworkCore;

namespace WebApi.Repositories
{
    public class SessionsRepository : AbstractRepository<Models.Session>
    {
        public SessionsRepository(TaskanaDb context)
            : base(context) { }

        public async Task<Models.Session?> FindByHashAsync(string hash, bool asNoTracking=true)
        {
            return await Query(asNoTracking)
                .Include(c => c.User)
                .Where(c => c.TokenHash == hash)
                .FirstOrDefaultAsync();
        }

    }
}
