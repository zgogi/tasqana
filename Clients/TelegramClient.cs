using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Tasqana.Clients
{
    public class TelegramClient
    {
        private readonly HttpClient _httpClient;
        private readonly string? _botToken;
        private readonly string _host;

        public TelegramClient(
            IConfiguration configuration
            )
        {
            _httpClient = new HttpClient(); 

            var config = configuration.GetSection("Telegram").GetChildren();
            _botToken = config.SingleOrDefault(e => e.Key == "BotToken")?.Value;
            if (_botToken == null) throw new Exception("Telegram bot is not configured");
            _host = $"https://api.telegram.org/bot{_botToken}";
        }

        public async Task<Models.http.Telegram.Message?> SendMessageAsync(Models.http.Telegram.SendRequest payload)
        {
            string url = $"{_host}/sendMessage";

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

        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            var filePath = await GetFileAsync(fileId);
            var content = await GetFileContentAsync(filePath);
            return content;
        }

        public async Task<string> GetFileAsync(string fileId)
        {
            string url = $"{_host}/getFile?file_id={fileId}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var resp = await response.Content.ReadFromJsonAsync<Models.http.Telegram.FileResponse>();
                if (resp?.ok != true) throw new Exception("Can't read file path " + resp?.description ?? "Unknown error");
                var ret = resp?.result?.file_path;
                if (ret == null) throw new Exception("Telegram did not return file name");
                return ret;
            }
            else
            {
                throw new Exception("Can't read file path: " + response.StatusCode);
            }
        }

        public async Task<Stream> GetFileContentAsync(string filePath)
        {
            var url = $"https://api.telegram.org/file/bot{_botToken}/{filePath}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) throw new Exception("Can't get file context from telegram: "+response.StatusCode);
            using Stream networkStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await networkStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream; 
        }


    }
}

