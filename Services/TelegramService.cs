using Tasqana.Clients;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;
using static Tasqana.Models.http.Telegram;
namespace Tasqana.Services
{
    public class TelegramService
    {
        private readonly SessionsService _sessions;
        private readonly UsersService _users;
        private readonly TelegramRepository _telegram;
        private readonly TelegramClient _client;
        private readonly TodosService _todos;
        private readonly TodoMediaService _todoMedia;
        private readonly string _dns;

        public TelegramService(
            SessionsService sessions,
            UsersService users,
            TelegramRepository telegram,
            TelegramClient client,
            TodosService todos,
            TodoMediaService todoMedia,
            IConfiguration configuration
            )
        {
            _sessions = sessions;
            _users = users;
            _telegram = telegram;
            _client = client;
            _todos = todos;
            _todoMedia = todoMedia;
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
            if (result.Text == "/start")
                return await ProcessStartMessageAsync(user, http);
            else if (result.Text != null)
                return await ProcessTodoMessageAsync(user, message);
            else
                return false;
        }

        public async Task<bool> SendMessageAsync(Models.User user, Models.http.Telegram.SendRequest content)
        {
            var msg = await _client.SendMessageAsync(content);
            if (msg == null) return false;
            var result = new Models.TelegramMessage(msg.message_id, user.Id, false, content.text);
            await _telegram.InsertAsync(result);
            return true;
        }

        private async Task<bool> ProcessStartMessageAsync(Models.User user, HttpContext http)
        {
            var auth = await _sessions.CreateSessionAsync(user, http, 5);
            var url = String.Format("https://{0}/login/?token={1}", _dns, auth.token);
            var text = String.Format("Welcome {0}\nPlease click button below to login", user.Name);
            var content = Models.http.Telegram.SendRequest.CreateLogin(user.TelegramId, text, "Login", url);
            return await SendMessageAsync(user, content);
        }

        private async Task<bool> ProcessTodoMessageAsync(Models.User user, Models.http.Telegram.Message message)
        {
            var todo = await _todos.InsertAsync(user, TodoDTO.FromString(message.text ?? message.caption ?? ""));
            if (message.photo != null)
            {
                var full = message.photo.MaxBy(e => e.width);
                var preview = message.photo.MinBy(e => e.width);
                if (full != null) {
                    var fileName = await _client.GetFileAsync(full.file_id);
                    using var fileContent = await _client.GetFileContentAsync(fileName);

                    if ((preview != null) && (preview != full))
                    {
                        var previewName = await _client.GetFileAsync(preview.file_id);
                        using var previewContent = await _client.GetFileContentAsync(previewName);
                        await _todoMedia.InsertAsync(todo, fileName.Replace('/', '_'), fileContent, previewContent);
                    }
                    else
                    {
                        await _todoMedia.InsertAsync(todo, fileName.Replace('/', '_'), fileContent);
                    }
                }
            }

            return true;
        }


       
    }
}
