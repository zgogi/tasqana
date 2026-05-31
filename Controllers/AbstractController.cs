using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    public abstract class AbstractController : ControllerBase
    {
        protected SessionsService _sessions;

        protected AbstractController(SessionsService sessions)
        {
            _sessions = sessions;
        }

        [NonAction]
        protected async Task<Models.User?> AuthenticateAsync()
        {
            var token = ExtractBearerToken();
            if (token == null) { return null; }
            return await _sessions.FindSessionAsync(token);
        }

        [NonAction]
        protected async Task<ActionResult> WithAuthenticationAsync(Func<Models.User, Task<ActionResult>> action)
        {
            var user = await AuthenticateAsync();
            if (user == null) return Unauthorized();
            return await action.Invoke(user);
        }

        [NonAction]
        protected long? ReadQueryLong(string name)
        {
            var str = Request.Query.FirstOrDefault(s => s.Key.ToLower() == name).Value;
            if (long.TryParse(str, out var result)) { return result; }
            return null;
        }

        private string? ExtractBearerToken()
        {
             // Ищем заголовок "Authorization"
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return null;
            }

            string headerValue = authHeader.ToString();

            // Проверяем, что он начинается с "Bearer " (длина слова с пробелом — 7 символов)
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring(7).Trim();
            }

            return null;
        }
    }
}