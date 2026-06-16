using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Tasqana.Extensions;
using Tasqana.Models;

namespace Tasqana.Repositories
{
    public abstract class AbstractRepository<T> where T : AbstractModel<T>
    {
        protected readonly TaskanaDb Context;
        protected readonly DbSet<T> Entities;

        protected AbstractRepository(TaskanaDb context)
        {
            Context = context;
            Entities = Context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(long id, bool asNoTracking = false)
        {
            var entity = await Query(asNoTracking).Where(m => m.Id == id).SingleOrDefaultAsync();
            return entity;
        }

        public async Task<int> CountAsync() { return await Entities.CountAsync(); }

        public virtual async Task<List<T>> GetAllAsync(bool asNoTracking = false)
        {
            var entities = await Query(asNoTracking)
                .ToListAsync();

            return entities;
        }

        public virtual IQueryable<T> Query(bool asNoTracking)
        {
            IQueryable<T> query = Entities;
            if (asNoTracking) query = query.AsNoTracking();
            return query;
        }

        public IQueryable<T> QueryById(long id, bool asNoTracking) { return Query(asNoTracking).Where(m => m.Id == id); }

        public async Task<T> InsertAsync(T entity)
        {
            Entities.Add(entity);
            await SaveChangesAsync();
            return entity;
        }
        public async Task DeleteByIdAsync(long id)
        {
            var item = await Entities.FindAsync(id);
            if (item == null) throw new NotFoundException();
            Entities.Remove(item);
            await SaveChangesAsync(); // Do not use ExecuteDeleteAsync() or file auto delete will not work
        }

        public async Task RemoveAsync(T item)
        {
            Entities.Remove(item);
            await SaveChangesAsync(); // Do not use ExecuteDeleteAsync() or file auto delete will not work
        }

        public async Task SaveChangesAsync() { await Context.SaveChangesAsync(); }
        
    }




}
