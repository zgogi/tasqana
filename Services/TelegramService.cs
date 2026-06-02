using Tasqana.Clients;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;
namespace Tasqana.Services
{
    public class TelegramService
    {
        private readonly SessionsService _sessions;
        private readonly UsersService _users;
        private readonly TelegramRepository _telegram;
        private readonly TelegramClient _client;
        private readonly TodosService _todos;
        private readonly string _dns;

        public TelegramService(
            SessionsService sessions,
            UsersService users,
            TelegramRepository telegram,
            TelegramClient client,
            TodosService todos,
            IConfiguration configuration
            )
        {
            _sessions = sessions;
            _users = users;
            _telegram = telegram;
            _client = client;
            _todos = todos;
            var config = configuration.GetSection("Host").GetChildren();
            _dns = config.SingleOrDefault(e => e.Key == "Dns")?.Value ?? "";
        }

        public async Task<List<Models.TelegramMessage>> GetAllAsync()
        {
            return await _telegram.GetAllAsync();
        }

        public async Task<Models.User?> GetOrCreateUserAsync(Models.http.Telegram.Message message)
        {
            if (message.from == null) return null;
            var user = await _users.GetByTelegramIdAsync(message.from.id);
            if (user != null) return user;
            var name = message.from?.full_name ?? "";
            user = await _users.InsertAsync(message.from?.id ?? 0, name, message.from?.username);
            return user;
        }

        public async Task<bool> ProcessMessageAsync(Models.User user, Models.http.Telegram.Message message, HttpContext http)
        {
            var result = message.ToMessage(user.Id);
            result = await _telegram.InsertAsync(result);
            result.User = user;
            return await ProcessMessageTextAsync(result, http);
        }

        public async Task<bool> SendMessageAsync(Models.User user, Models.http.Telegram.SendRequest content)
        {
             var msg = await _client.SendMessageAsync(content);
            if (msg == null) return false;

            var result = new Models.TelegramMessage(msg.message_id, user.Id, false, content.text);
            await _telegram.InsertAsync(result);
            return true;
        }

        
        private async Task<bool> ProcessMessageTextAsync(Models.TelegramMessage message, HttpContext http)
        {
            if (message.Text == "/start")
            {
                var auth = await _sessions.CreateSessionAsync(message.User, http, 5);
                var url = String.Format("https://{0}/login/?token={1}", _dns, auth.token);
                var text = String.Format("Welcome {0}\nPlease click button below to login", message.User.Name);
                var content = Models.http.Telegram.SendRequest.CreateLogin(message.User.TelegramId, text, "Login", url);
                return await SendMessageAsync(message.User, content);
            } 
            else if (message.Text != null)
            {
                await _todos.InsertAsync(message.User, TodoCreateDTO.FromString(message.Text));
                return true;
            } else
            {
                return false;
            }
                
        }
    }
}
