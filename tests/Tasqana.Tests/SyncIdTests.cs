using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Tasqana.Extensions;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;
using Tasqana.Services;

namespace Tasqana.Tests;

public class SyncIdTests
{
    [Test]
    public void NewEntitiesHaveDistinctNonEmptySyncIds()
    {
        ISyncEntity[] items = [new Todo { Title = "Task" }, new Category(), new CheckItem(), new TodoFile(), new Todo { Title = "Other" }];
        Assert.That(items.Select(x => x.SyncId), Is.Unique.And.Not.Contains(Guid.Empty));
    }

    [Test]
    public void ChecklistCanBeCreatedAndUpdatedBySyncIdWithoutDuplicates()
    {
        var service = new CheckItemService(null!, null!);
        var todo = new Todo { Id = 5, Title = "Task" };
        var syncId = Guid.NewGuid();
        service.UpdateList(todo, [new() { syncId = syncId, title = "First" }]);
        var original = todo.CheckItems.Single();
        original.Id = 12;
        service.UpdateList(todo, [new() { syncId = syncId, title = "Changed", is_completed = true }]);
        Assert.Multiple(() =>
        {
            Assert.That(todo.CheckItems.Single(), Is.SameAs(original));
            Assert.That(original.SyncId, Is.EqualTo(syncId));
            Assert.That(original.Title, Is.EqualTo("Changed"));
            Assert.That(original.IsCompleted, Is.True);
        });
    }

    [Test]
    public void ChecklistRejectsMismatchingIdsBeforeChangingCollection()
    {
        var existing = new CheckItem { Id = 1, Title = "Keep" };
        var todo = new Todo { Title = "Task", CheckItems = [existing] };
        var service = new CheckItemService(null!, null!);
        Assert.Throws<InvalidRequestException>(() => service.UpdateList(todo,
            [new() { id = 1, syncId = Guid.NewGuid(), title = "Wrong" }]));
        Assert.That(todo.CheckItems.Single(), Is.SameAs(existing));
        Assert.That(existing.Title, Is.EqualTo("Keep"));
    }

    [Test]
    public void ChecklistRejectsDuplicateSyncIds()
    {
        var syncId = Guid.NewGuid();
        var todo = new Todo { Title = "Task" };
        Assert.Throws<InvalidRequestException>(() => new CheckItemService(null!, null!).UpdateList(todo,
            [new() { syncId = syncId }, new() { syncId = syncId }]));
        Assert.That(todo.CheckItems, Is.Empty);
    }

    [Test]
    public void EmptySyncIdIsRejected()
        => Assert.Throws<InvalidRequestException>(() => SyncIdentity.Validate(Guid.Empty));

    [Test]
    public void IdentityLookupRequiresBothIdentifiersToMatchAndPreservesOwnerFilter()
    {
        var owned = new Todo { Id = 1, UserId = 7, Title = "Mine" };
        var other = new Todo { Id = 2, UserId = 8, Title = "Other" };
        var query = new[] { owned, other }.AsQueryable().Where(x => x.UserId == 7);
        Assert.That(query.WithIdentity(null, owned.SyncId).Single(), Is.SameAs(owned));
        Assert.That(query.WithIdentity(owned.Id, other.SyncId), Is.Empty);
        Assert.That(query.WithIdentity(null, other.SyncId), Is.Empty);
        Assert.Throws<InvalidRequestException>(() => query.WithIdentity(null, null));
    }

    [Test]
    public void ResponseContainsSyncIdsForTaskAndItsReferences()
    {
        var category = new Category { Id = 2 };
        var task = new Todo
        {
            Id = 3, Title = "Task", CategoryId = category.Id, Category = category,
            CheckItems = [new CheckItem { Id = 4 }],
            Files = [new TodoFile { Id = 5, FileName = "file" }]
        };
        var dto = new TodoDTO(task, name => name);
        Assert.Multiple(() =>
        {
            Assert.That(dto.syncId, Is.EqualTo(task.SyncId));
            Assert.That(dto.categorySyncId, Is.EqualTo(category.SyncId));
            Assert.That(dto.check_items!.Single().syncId, Is.EqualTo(task.CheckItems.Single().SyncId));
            Assert.That(dto.files.Single().syncId, Is.EqualTo(task.Files.Single().SyncId));
        });
    }

    [Test]
    public void ClientTaskSyncIdSurvivesConversion()
    {
        var syncId = Guid.NewGuid();
        var task = new TodoDTO { syncId = syncId }.ToTodo(new User { Id = 1, Name = "User", TelegramId = 123 });
        Assert.That(task.SyncId, Is.EqualTo(syncId));
    }

    [Test]
    public void MigrationMatchesModelAndBackfillsOnlySyncColumns()
    {
        using var db = CreateContext();
        Assert.That(db.Database.HasPendingModelChanges(), Is.False);
        var script = db.GetService<IMigrator>().GenerateScript("20260621034553_RenamedMediaToFiles", "20261018000000_AddSyncIds");
        foreach (var table in new[] { "categories", "todos", "todos_checkitems", "todo_files" })
        {
            Assert.That(script, Does.Contain($"ALTER TABLE {table} ADD \"SyncId\" uuid NOT NULL DEFAULT (gen_random_uuid())"));
            Assert.That(script, Does.Contain($"CREATE UNIQUE INDEX \"IX_{table}_SyncId\""));
        }
        Assert.That(script, Does.Not.Contain("DROP TABLE").And.Not.Contain("DROP COLUMN"));
    }

    [Test]
    public async Task RepeatedCreateReturnsExistingTaskWithoutOverwritingIt()
    {
        using var db = CreateContext();
        var user = new User { Id = 7, Name = "User", TelegramId = 123 };
        var existing = new Todo { Id = 5, UserId = user.Id, Title = "Current title" };
        var service = new TodosService(new ExistingTodos(db, existing), null!, null!);

        var result = await service.InsertAsync(user, new TodoDTO { syncId = existing.SyncId, title = "Old retry" });

        Assert.That(result, Is.SameAs(existing));
        Assert.That(result.Title, Is.EqualTo("Current title"));
    }

    [Test]
    public async Task RepeatedCreateReturnsExistingCategory()
    {
        using var db = CreateContext();
        var user = new User { Id = 7, Name = "User", TelegramId = 123 };
        var existing = new Category { Id = 5, UserId = user.Id, Title = "Current title" };
        var service = new CategoriesService(new ExistingCategories(db, existing));

        var result = await service.InsertAsync(user, "Old retry", syncId: existing.SyncId);

        Assert.That(result.syncId, Is.EqualTo(existing.SyncId));
        Assert.That(result.id, Is.EqualTo(existing.Id));
        Assert.That(result.title, Is.EqualTo("Current title"));
    }

    [Test]
    public void IdentityQueriesTranslateToPostgresSql()
    {
        using var db = CreateContext();
        var syncId = Guid.NewGuid();
        var query = db.Todos.Where(x => x.UserId == 7).WithIdentity(null, syncId).ToQueryString();
        Assert.That(query, Does.Contain("\"UserId\" = 7").And.Contain("\"SyncId\" ="));
    }

    private sealed class ExistingTodos(TaskanaDb db, Todo item) : TodosRepository(db)
    {
        public override IQueryable<Todo> Query(bool asNoTracking) => new AsyncQuery<Todo>(new[] { item }.AsQueryable());
    }

    private sealed class ExistingCategories(TaskanaDb db, Category item) : CategoriesRepository(db)
    {
        public override IQueryable<Category> Query(bool asNoTracking) => new AsyncQuery<Category>(new[] { item }.AsQueryable());
    }

    private sealed class AsyncQuery<T>(IQueryable<T> inner) : IQueryable<T>
    {
        public Type ElementType => typeof(T);
        public Expression Expression => inner.Expression;
        public IQueryProvider Provider => new AsyncProvider(inner.Provider);
        public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class AsyncProvider(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => inner.CreateQuery(expression);
        public IQueryable<T> CreateQuery<T>(Expression expression) => new AsyncQuery<T>(inner.CreateQuery<T>(expression));
        public object? Execute(Expression expression) => inner.Execute(expression);
        public T Execute<T>(Expression expression) => inner.Execute<T>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var result = inner.Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(typeof(TResult).GetGenericArguments()[0]).Invoke(null, [result])!;
        }
    }

    private static TaskanaDb CreateContext() => new(
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PostgresConnectionData:Host"] = "localhost",
            ["PostgresConnectionData:Database"] = "unused"
        }).Build(), NullLogger<TaskanaDb>.Instance, new DeleteFileS3Interceptor(null!));
}
