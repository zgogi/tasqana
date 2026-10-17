using Tasqana.Models.http;

namespace Tasqana.Tests;

public class TelegramMessageTests
{
    [TestCase("Text", "Caption", "Text")]
    [TestCase(null, "Caption", "Caption")]
    [TestCase(null, null, "")]
    public void ToMessage_UsesTextOrPhotoCaptionAndSetsIncomingOwner(string? text, string? caption, string expected)
    {
        var message = new Telegram.Message { message_id = 42, text = text, caption = caption };
        var result = message.ToMessage(7);
        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Is.EqualTo(expected));
            Assert.That(result.UserId, Is.EqualTo(7));
            Assert.That(result.MessageId, Is.EqualTo(42));
            Assert.That(result.IsIncoming, Is.True);
        });
    }

    [Test]
    public void CreateLogin_PutsLoginUrlInInlineButtonForRecipient()
    {
        var request = Telegram.SendRequest.CreateLogin(123456789, "Welcome", "Login", "https://example.test/login/?token=test");
        Assert.Multiple(() =>
        {
            Assert.That(request.chat_id, Is.EqualTo(123456789));
            Assert.That(request.text, Is.EqualTo("Welcome"));
            Assert.That(request.reply_markup!.inline_keyboard.Single().Single().text, Is.EqualTo("Login"));
            Assert.That(request.reply_markup.inline_keyboard.Single().Single().url, Is.EqualTo("https://example.test/login/?token=test"));
        });
    }
}
