using Tasqana.Models;
using Tasqana.Models.http;

namespace Tasqana.Tests;

public class TodoDtoTests
{
    [TestCase("Task", "Task", null)]
    [TestCase("Task\nDetails", "Task", "Details")]
    [TestCase("Task. Details", "Task", "Details")]
    [TestCase("Task\nFirst\nSecond", "Task", "First\nSecond")]
    public void FromString_SplitsTitleAndDescription(string text, string title, string? description)
    {
        var result = TodoDTO.FromString(text);
        Assert.Multiple(() =>
        {
            Assert.That(result.title, Is.EqualTo(title));
            Assert.That(result.description, Is.EqualTo(description));
        });
    }

    [TestCase(null, Priority.Lowest)]
    [TestCase(4, Priority.Highest)]
    [TestCase(99, Priority.Lowest)]
    public void ToTodo_AssignsOwnerCategoryAndValidPriority(int? priority, Priority expected)
    {
        var user = new User { Id = 7, TelegramId = 123456789, Name = "Test" };
        var dto = new TodoDTO { title = "Task", description = " Details ", category_id = 12, priority = priority };
        var result = dto.ToTodo(user);
        Assert.Multiple(() =>
        {
            Assert.That(result.UserId, Is.EqualTo(user.Id));
            Assert.That(result.CategoryId, Is.EqualTo(12));
            Assert.That(result.Title, Is.EqualTo("Task"));
            Assert.That(result.Description, Is.EqualTo("Details"));
            Assert.That(result.Priority, Is.EqualTo(expected));
        });
    }
}
