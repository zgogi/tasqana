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
        private readonly CategoriesRepository _categories;
        private readonly TodosRepository _todoRepository;
        private readonly TelegramCategoryMenu _categoryMenu;

        public TelegramService(
            SessionsService sessions,
            UsersService users,
            TelegramRepository telegram,
            TelegramClient client,
            TodosService todos,
            TodoMediaService todoMedia,
            IConfiguration configuration,
            CategoriesRepository categories,
            TodosRepository todoRepository,
            TelegramCategoryMenu categoryMenu
            )
        {
            _sessions = sessions;
            _users = users;
            _telegram = telegram;
            _client = client;
            _todos = todos;
            _todoMedia = todoMedia;
            _categories = categories;
            _todoRepository = todoRepository;
            _categoryMenu = categoryMenu;
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
            var text = result.Text?.TrimStart() ?? "";
            if (text.StartsWith('/'))
            {
                var command = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0].Split('@')[0];
                return command == "/start" && await ProcessStartMessageAsync(user, http);
            }
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

        public async Task ProcessCallbackAsync(CallbackQuery callback)
        {
            string? answer = null;
            try
            {
                if (callback.from == null || callback.message?.chat == null ||
                    !_categoryMenu.TryParse(callback.data, out var action) || action!.Command != "move" ||
                    !long.TryParse(action.Context, out var todoId) || todoId <= 0)
                {
                    answer = "Эта кнопка больше недоступна.";
                    return;
                }
                var user = await _users.GetByTelegramIdAsync(callback.from.id);
                if (user == null) { answer = "Сначала отправьте /start."; return; }
                var todo = await _todoRepository.GetByIdAsync(user, todoId, true);
                var categories = await _categories.GetByUser(user);
                var category = categories.SingleOrDefault(c => c.Id == action.CategoryId);
                if (todo == null || (action.CategoryId.HasValue && category == null))
                {
                    answer = "Задача или категория недоступна.";
                    return;
                }
                var heading = "Выберите категорию для задачи: " + ShortTitle(todo.Title);
                var parentId = action.CategoryId;
                if (action.Select)
                {
                    await _todos.UpdateAsync(user, new TodoDTO { id = todo.Id, category_id = category!.Id });
                    answer = "Задача перемещена.";
                    heading = "Задача «" + ShortTitle(todo.Title) + "» перемещена в «" + ShortTitle(category.Title) + "».";
                    parentId = category.ParentId;
                }
                var page = _categoryMenu.Build(categories, parentId, action.Command, action.Context, heading);
                var edited = await _client.EditMessageAsync(new EditMessageRequest(callback.message.chat.id,
                    callback.message.message_id, page.Text, page.Markup));
                if (!edited) answer = action.Select
                    ? "Задача перемещена, но меню не обновилось."
                    : "Не удалось обновить меню. Попробуйте ещё раз.";
            }
            catch
            {
                answer = "Не удалось выполнить действие. Попробуйте ещё раз.";
                throw;
            }
            finally
            {
                await _client.AnswerCallbackAsync(callback.id, answer);
            }
        }

        private static string ShortTitle(string title) => title.Length > 200 ? title[..200] + "…" : title;

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

            var categories = await _categories.GetByUser(user);
            var page = _categoryMenu.Build(categories, null, "move",
                todo.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "Задача сохранена: " + ShortTitle(todo.Title) + "\nВыберите категорию:");
            return await SendMessageAsync(user, new SendRequest(message.chat?.id ?? user.TelegramId, page.Text, page.Markup));
        }


       
    }
}
