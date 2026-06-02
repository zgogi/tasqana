using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tasqana.Repositories;

namespace Tasqana.Models
{
    public abstract class AbstractModel<T> : IBeforeSaveBehavior where T : class
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void BeforeSave(Repositories.TaskanaDb dbContext)
        {
            if (dbContext.Entry((this as T)!).State == EntityState.Added)
            {
                CreatedAt = DateTime.UtcNow;
                UpdatedAt = DateTime.UtcNow;
            }

            if (dbContext.Entry((this as T)!).State == EntityState.Modified)
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    public interface IOrderable
    {
        public long Id { get; set; }
        public int Order { get; set; }
    }

    public interface IBeforeSaveBehavior
    {
        public void BeforeSave(TaskanaDb dbContext);
    }
}
