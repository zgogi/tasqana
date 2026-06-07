using Microsoft.AspNetCore.Mvc;
using Tasqana.Services;

namespace Tasqana.Controllers
{
    [ApiController, Route("api/v1.0/admin")]
    public class AdminController : AbstractController
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly CategoriesService _categories;
        private readonly UsersService _users;
        private readonly TodosService _todos;
        private readonly TelegramService _telegram;
        public AdminController(
            SessionsService sessions,
            ILogger<CategoriesController> logger,
            CategoriesService categories,
            UsersService users,
            TodosService todos,
            TelegramService telegram
            ) : base(sessions)
        {
            _logger = logger;
            _categories = categories;
            _users = users;
            _todos = todos;
            _telegram = telegram;
        }

        [HttpGet, Route("users")]
        public async Task<ActionResult> GetUsers()
        {
            return await WithAuthenticationAsync(async user => {
                if (!user.IsAdmin) return Unauthorized();
                var result = await _users.GetAllAsync();
                return Ok(result.Select(e => new Models.http.UserDTO(e)));
            });

        }

        [HttpGet, Route("sessions")]
        public async Task<ActionResult> GetSessions()
        {
            return await WithAuthenticationAsync(async user => {
                if (!user.IsAdmin) return Unauthorized();
                var result = await _sessions.GetAllAsync();
                return Ok(result.Select(e => new Models.http.SessionDTO(e)));
            });
        }

        [HttpGet, Route("todos")]
        public async Task<ActionResult> GetTodos()
        {
            return await WithAuthenticationAsync(async user => {
                if (!user.IsAdmin) return Unauthorized();
                var result = await _todos.GetAllAsync();
                return Ok(result);
            });
        }

        [HttpGet, Route("messages")]
        public async Task<ActionResult> GetMessages()
        {
            return await WithAuthenticationAsync(async user => {
                if (!user.IsAdmin) return Unauthorized();
                var result = await _telegram.GetAllAsync();
                return Ok(result.Select(e => new Models.http.TelegramMessageDTO(e)));
            });
        }
    }
}
