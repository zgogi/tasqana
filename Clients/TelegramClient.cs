using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Tasqana.Clients
{
    public class TelegramClient
    {
        private readonly HttpClient _httpClient;
        private readonly string? _botToken;
        

        public TelegramClient(
            IConfiguration configuration
            )
        {
            _httpClient = new HttpClient(); 

            var config = configuration.GetSection("Telegram").GetChildren();
            _botToken = config.SingleOrDefault(e => e.Key == "BotToken")?.Value;
            
        }

        public async Task<Models.http.Telegram.Message?> SendMessageAsync(Models.http.Telegram.SendRequest payload)
        {
            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload);
                if (response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadFromJsonAsync<Models.http.Telegram.Response>();
                    if (resp?.ok == true) return resp?.result;
                    else return null;
                } 
                string errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Telegram API Error: {errorResponse}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Network error while sending Telegram message: {ex.Message}");
                return null;
            }
        }

    }
}

