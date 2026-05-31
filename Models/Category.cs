using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models
{
    public class Category : AbstractModel<Category>
    {
        public long? ParentId { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }

        
        public Category? Parent { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}
