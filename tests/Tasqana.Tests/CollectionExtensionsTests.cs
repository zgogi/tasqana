using Tasqana.Extensions;
using Tasqana.Models;

namespace Tasqana.Tests;

public class CollectionExtensionsTests
{
    [TestCase(3, 1, new long[] { 3, 1, 2 })]
    [TestCase(1, 3, new long[] { 2, 1, 3 })]
    [TestCase(1, null, new long[] { 2, 3, 1 })]
    [TestCase(1, 99, new long[] { 2, 3, 1 })]
    [TestCase(99, 1, new long[] { 1, 2, 3 })]
    public void MoveBefore_ReordersItemsAndKeepsConsecutiveOrder(long id, long? beforeId, long[] expected)
    {
        var items = Enumerable.Range(1, 3).Select(i => new CheckItem { Id = i, Order = i - 1 }).ToList();
        items.MoveBefore(id, beforeId);
        Assert.Multiple(() =>
        {
            Assert.That(items.Select(x => x.Id), Is.EqualTo(expected));
            Assert.That(items.Select(x => x.Order), Is.EqualTo(new[] { 0, 1, 2 }));
        });
    }

    [Test]
    public void MoveBefore_EmptyListDoesNotThrow()
    {
        var items = new List<CheckItem>();
        Assert.DoesNotThrow(() => items.MoveBefore(1, null));
        Assert.That(items, Is.Empty);
    }
}
