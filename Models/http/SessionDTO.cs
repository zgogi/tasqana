using Tasqana.Extensions;

namespace Tasqana.Models.http
{
    public class SessionDTO
    {
        public long id { get; set; }
        public string user_name { get; set; } = null!;
        public string? tg_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime expired_at { get; set; }

        public SessionDTO(Models.Session source)
        {
            id = source.Id;
            user_name = source.User.Name;
            tg_name = source.User.TelegramUsername?.ToTelegramUsername();
            created_at = source.CreatedAt;
            expired_at = source.ExpiredAt;
        }
    }
}
