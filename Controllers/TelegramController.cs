using Microsoft.AspNetCore.Mvc;
using Tasqana.Services;

namespace Tasqana.Controllers
{ 
    [ApiController]
    [Route("api/v1.0/telegram")]
    public class TelegramController : ControllerBase
    {
        private readonly ILogger<TelegramController> _logger;
        private readonly TelegramService _telegram;
        private readonly string? _secret;
        public TelegramController(
            ILogger<TelegramController> logger,
            TelegramService telegram,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _telegram = telegram;
            var section = configuration.GetSection("Telegram").GetChildren();
            _secret = section.SingleOrDefault(e => e.Key == "WebhookSecret")?.Value;
        }

        [HttpPost, Route("webhook")]
        public async Task<ActionResult> WebHook(Models.http.Telegram.Update update)
        {
            var secret = HttpContext.Request.Headers
                   .SingleOrDefault(v => v.Key.ToLower() == "x-telegram-bot-api-secret-token")
                   .Value
                   .FirstOrDefault();
            if (secret != _secret) return Unauthorized();

            if (update.message == null) return Ok();
            var user = await _telegram.GetOrCreateUserAsync(update.message);
            if (user == null) return Ok();
            await _telegram.ProcessMessageAsync(user, update.message, HttpContext);
            return Ok();
        }




    }
}
