using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tasqana.Clients;
using Tasqana.Models.http;
using Tasqana.Services;

namespace Tasqana.Tests;

public class TelegramCallbackTests
{
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<(string Path, string Body)> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.RequestUri!.AbsolutePath, await request.Content!.ReadAsStringAsync(cancellationToken)));
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"ok\":true,\"result\":true}", Encoding.UTF8, "application/json") };
        }
    }

    private static IConfiguration Config() => new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?> { ["Telegram:BotToken"] = "test-token" }).Build();

    [Test]
    public async Task EditTargetsTheCallbackMessageAndIncludesNewKeyboard()
    {
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler);
        var client = new TelegramClient(Config(), http);
        var markup = new Telegram.ReplyMarkup
        {
            inline_keyboard = [[Telegram.InlineKeyboardButton.Callback("Back", "cat|move|42|o|0")]]
        };
        Assert.That(await client.EditMessageAsync(new(7, 123, "Children", markup)), Is.True);
        using var body = JsonDocument.Parse(handler.Requests.Single().Body);
        Assert.That(handler.Requests.Single().Path, Does.EndWith("/editMessageText"));
        Assert.That(body.RootElement.GetProperty("message_id").GetInt64(), Is.EqualTo(123));
        Assert.That(body.RootElement.GetProperty("chat_id").GetInt64(), Is.EqualTo(7));
        Assert.That(body.RootElement.GetProperty("reply_markup").GetProperty("inline_keyboard")[0][0]
            .GetProperty("callback_data").GetString(), Is.EqualTo("cat|move|42|o|0"));
    }

    [TestCase("invalid")]
    [TestCase("cat|other-command|42|s|1")]
    [TestCase("cat|move|not-a-task|s|1")]
    public async Task InvalidCallbacksAreAcknowledgedWithoutAccessingTasks(string data)
    {
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler);
        var service = new TelegramService(null!, null!, null!, new TelegramClient(Config(), http),
            null!, null!, Config(), null!, null!, new TelegramCategoryMenu());
        await service.ProcessCallbackAsync(new Telegram.CallbackQuery
        {
            id = "callback-id", from = new() { id = 7 },
            message = new() { message_id = 123, chat = new() { id = 7 } }, data = data
        });
        Assert.That(handler.Requests.Single().Path, Does.EndWith("/answerCallbackQuery"));
        using var body = JsonDocument.Parse(handler.Requests.Single().Body);
        Assert.That(body.RootElement.GetProperty("callback_query_id").GetString(), Is.EqualTo("callback-id"));
        Assert.That(body.RootElement.GetProperty("text").GetString(), Is.Not.Empty);
    }
}
