using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Models
{
    public class User : AbstractModel<User>
    {
        public required string Name { get; set; }
        public required long TelegramId { get; set; }
        public string? TelegramUsername { get; set; }
        public bool IsAdmin { get; set; }
        public string? Language { get; set; }

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public IEnumerable<Todo> Todos { get; set; } = new List<Todo>();

        public IEnumerable<Session> Sessions { get; set; } = new List<Session>();

        public IEnumerable<TelegramMessage> Messages { get; set; } = new List<TelegramMessage>();


    }
}
