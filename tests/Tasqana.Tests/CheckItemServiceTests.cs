using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Services;

namespace Tasqana.Tests;

public class CheckItemServiceTests
{
    private readonly CheckItemService service = new(null!, null!);

    [Test]
    public void UpdateList_AddsNewItemsAndAssignsParentAndOrder()
    {
        var todo = new Todo { Id = 15, Title = "Task" };

        service.UpdateList(todo, [new() { title = "First" }, new() { title = "Second", is_completed = true }]);

        Assert.Multiple(() =>
        {
            Assert.That(todo.CheckItems.Select(x => x.Title), Is.EqualTo(new[] { "First", "Second" }));
            Assert.That(todo.CheckItems.Select(x => x.TodoId), Is.All.EqualTo(15));
            Assert.That(todo.CheckItems.Select(x => x.Order), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(todo.CheckItems.Select(x => x.IsCompleted), Is.EqualTo(new[] { false, true }));
        });
    }

    [Test]
    public void UpdateList_UpdatesRetainedItemsRemovesMissingItemsAndReorders()
    {
        var retained = new CheckItem { Id = 2, Title = "Old", Order = 1 };
        var todo = new Todo
        {
            Id = 15, Title = "Task",
            CheckItems = [new() { Id = 1, Title = "Remove", Order = 0 }, retained]
        };

        service.UpdateList(todo, [new() { title = "New" }, new() { id = 2, title = "Updated", is_completed = true }]);

        Assert.Multiple(() =>
        {
            Assert.That(todo.CheckItems, Has.Count.EqualTo(2));
            Assert.That(todo.CheckItems[1], Is.SameAs(retained));
            Assert.That(retained.Title, Is.EqualTo("Updated"));
            Assert.That(retained.IsCompleted, Is.True);
            Assert.That(todo.CheckItems.Select(x => x.Order), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(todo.CheckItems.Any(x => x.Id == 1), Is.False);
        });
    }

    [Test]
    public void UpdateList_EmptyListClearsChecklist()
    {
        var todo = new Todo { Title = "Task", CheckItems = [new() { Id = 1, Title = "Item" }] };
        service.UpdateList(todo, []);
        Assert.That(todo.CheckItems, Is.Empty);
    }
}
