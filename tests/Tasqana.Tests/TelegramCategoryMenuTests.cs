using System.Text;
using System.Text.Json;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Services;

namespace Tasqana.Tests;

public class TelegramCategoryMenuTests
{
    private readonly TelegramCategoryMenu _menu = new();
    private static Category[] Tree() =>
    [
        new() { Id = 1, Title = "Work", Order = 1 },
        new() { Id = 2, Title = "Personal", Order = 0 },
        new() { Id = 3, Title = "Project", ParentId = 1 },
        new() { Id = 4, Title = "Development", ParentId = 3 }
    ];

    [Test]
    public void RootShowsOnlyRootsInOrderWithOpenButtonForBranches()
    {
        var rows = _menu.Build(Tree(), null, "move", "42", "Saved").Markup.inline_keyboard;
        Assert.That(rows.Select(r => r[0].text), Is.EqualTo(new[] { "Personal", "Work" }));
        Assert.That(rows[0], Has.Count.EqualTo(1));
        Assert.That(rows[1].Select(b => b.text), Is.EqualTo(new[] { "Work", "Открыть" }));
        Assert.That(rows[1][0].callback_data, Is.EqualTo("cat|move|42|s|1"));
        Assert.That(rows[1][1].callback_data, Is.EqualTo("cat|move|42|o|1"));
    }

    [TestCase(1, "Project", "cat|move|42|o|0")]
    [TestCase(3, "Development", "cat|move|42|o|1")]
    public void SubmenuShowsDirectChildrenAndBackToParent(long parentId, string child, string back)
    {
        var rows = _menu.Build(Tree(), parentId, "move", "42", "Select").Markup.inline_keyboard;
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0][0].text, Is.EqualTo(child));
        Assert.That(rows[1].Single().callback_data, Is.EqualTo(back));
    }

    [Test]
    public void ArbitraryCommandAndContextSurviveNavigation()
    {
        var data = _menu.Build(Tree(), 3, "filter", "custom-context", "Choose")
            .Markup.inline_keyboard.Last().Single().callback_data;
        Assert.That(_menu.TryParse(data, out var action), Is.True);
        Assert.That(action, Is.EqualTo(new CategoryMenuAction("filter", "custom-context", false, 1)));
    }

    [TestCase(null)]
    [TestCase("cat|move|42|s|0")]
    [TestCase("cat|move|42|s|-1")]
    [TestCase("cat|move|42|x|1")]
    [TestCase("cat||42|o|1")]
    [TestCase("cat|move||o|1")]
    [TestCase("cat|move|42|o|1|extra")]
    public void InvalidCallbackIsRejected(string? data)
        => Assert.That(_menu.TryParse(data, out _), Is.False);

    [Test]
    public void CallbackLimitUsesUtf8Bytes()
    {
        Assert.Throws<ArgumentException>(() => _menu.Encode("move", new string('я', 30), true, 1));
        var data = _menu.Encode("move", long.MaxValue.ToString(), true, long.MaxValue);
        Assert.That(Encoding.UTF8.GetByteCount(data), Is.LessThanOrEqualTo(64));
        Assert.That(_menu.TryParse(data, out _), Is.True);
    }

    [Test]
    public void MissingCategoryIsNotTreatedAsRoot()
        => Assert.Throws<KeyNotFoundException>(() => _menu.Build(Tree(), 99, "move", "42", "Select"));

    [Test]
    public void EmptyTreeStillConfirmsTaskWasSaved()
    {
        var page = _menu.Build([], null, "move", "42", "Задача сохранена");
        Assert.That(page.Text, Does.Contain("Задача сохранена").And.Contain("Категорий пока нет"));
        Assert.That(page.Markup.inline_keyboard, Is.Empty);
    }

    [Test]
    public void CallbackButtonHasNoUrlAndWebhookContainsOriginatingMessage()
    {
        var json = JsonSerializer.Serialize(Telegram.InlineKeyboardButton.Callback("Category", "cat|move|42|s|1"));
        Assert.That(json, Does.Not.Contain("\"url\""));
        var update = JsonSerializer.Deserialize<Telegram.Update>("""
            {"callback_query":{"id":"query","from":{"id":7},"message":{"message_id":123,"chat":{"id":7}},"data":"cat|move|42|s|1"}}
            """);
        Assert.That(update!.callback_query!.message!.message_id, Is.EqualTo(123));
        Assert.That(update.callback_query.message.chat!.id, Is.EqualTo(7));
    }
}
