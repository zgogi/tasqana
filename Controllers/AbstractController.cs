using Microsoft.AspNetCore.Mvc;
using Tasqana.Extensions;
using Tasqana.Services;

namespace Tasqana.Controllers
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
            try
            {
                return await action.Invoke(user);
            } catch(HttpException ex)
            {
                return ex.Result;
            }
            
        }

        [NonAction]
        protected long? ReadQueryLong(string name)
        {
            var str = Request.Query.FirstOrDefault(s => s.Key.ToLower() == name).Value;
            if (long.TryParse(str, out var result)) { return result; }
            return null;
        }

        [NonAction]
        protected int? ReadQueryInt(string name)
        {
            var str = Request.Query.FirstOrDefault(s => s.Key.ToLower() == name).Value;
            if (int.TryParse(str, out var result)) { return result; }
            return null;
        }

        [NonAction]
        protected bool? ReadQueryBool(string name)
        {
            var str = Request.Query.FirstOrDefault(s => s.Key.ToLower() == name).Value.ToString();
            if ((str == "1") || (str.ToLower() == "true")) return true;
            if ((str == "0") || (str.ToLower() == "false")) return false;
            return null;
        }

        private string? ExtractBearerToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return null;
            }

            string headerValue = authHeader.ToString();

            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring(7).Trim();
            }

            return null;
        }
    }


}