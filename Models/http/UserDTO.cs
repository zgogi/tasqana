using Tasqana.Extensions;

namespace Tasqana.Models.http
{
    public class AuthResponseDTO
    {
        public string name { get; set; } = null!;
        public string token { get; set; } = null!;
        public bool is_admin { get; set; }
        public string expired_at { get; set; } = null!;

        public AuthResponseDTO(Models.User user, string token, DateTime expiredAt)
        {
            this.name = user.Name;
            this.token = token;
            this.is_admin = user.IsAdmin;
            this.expired_at = expiredAt.ToString("O");
        }
    }

    public class UserDTO
    {
        public long id { get; set; }
        public string user_name { get; set; } = null!;
        public long tg_id { get; set; }
        public string? tg_name { get; set; }
        public string? language { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }

        public UserDTO(Models.User source)
        {
            id = source.Id;
            user_name = source.Name;
            tg_id = source.TelegramId;
            tg_name = source.TelegramUsername?.ToTelegramUsername();
            language = source.Language;
            created_at = source.CreatedAt;
            modified_at = source.UpdatedAt;
        }
    }
}
