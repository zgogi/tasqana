using Tasqana.Extensions;
using static Tasqana.Models.http.Telegram;

namespace Tasqana.Models.http
{
   
    public class TodoDeleteDTO
    {
        public long id { get; set; }
    }

    public class TodoDTO
    {
        public long? id { get; set; }
        public string? title { get; set; } = null;
        public string? description { get; set; } = null;
        public long? category_id { get; set; } = null;
        public DateTime? created_at { get; set; }
        public DateTime? modified_at { get; set; }
        public int? state { get; set; }
        public int? priority { get; set; }
        public IEnumerable<CheckItemDTO> check_items { get; set; } = new List<CheckItemDTO>();

        public TodoDTO() { }
        public TodoDTO(Todo source)
        {
            id = source.Id;
            title = source.Title;
            description = source.Description;
            category_id = source.CategoryId;
            state = ((int)source.State);
            priority = ((int)source.Priority);
            check_items = source.CheckItems.Select(e => new CheckItemDTO(e));
            created_at = source.CreatedAt;
            modified_at = source.UpdatedAt;
        }

        public Todo ToTodo(Models.User user)
        {
            return new Models.Todo
            {
                UserId = user.Id,
                CategoryId = this.category_id,
                Title = this.title ?? "",
                Description = this.description?.Trim()?.NullIfEmpty(),
                Priority = this.priority.ToEnum<Priority>() ?? Priority.Lowest,
            };
        }

        public static TodoDTO FromString(string title)
        {
            var lines = title.Split("\n");
            if (lines.Length == 1)
                lines = title.Split(". ");
            if (lines.Length == 1)
                return new TodoDTO { title = title };

            return new TodoDTO
            {
                title = lines[0].Trim(),
                description = lines
                    .Take(new Range(1, lines.Length))
                    .Aggregate((current, next) => current + "\n" + next.Trim()),
            };
        }
    }

    public class TodoExtDTO : TodoDTO
    {
        public string user_name { get; set; } = null!;
        public string? category { get; set; }
        public int order { get; set; }
        public TodoExtDTO(Todo source):base(source)
        {
            user_name = source.User.Name;
            category = source.Category?.Title;
            order = source.Order;
        }
    }

}
