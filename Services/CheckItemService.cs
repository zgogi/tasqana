using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.CompilerServices;
using Tasqana.Extensions;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class CheckItemService
    {
        private readonly TodosRepository _todos;
        private readonly CheckItemRepository _checkitems;

        public CheckItemService(
            TodosRepository todos,
            CheckItemRepository checkitems)
        {
            _todos = todos;
            _checkitems = checkitems;
        }

       /* public async Task<Models.http.TodoDTO> CreateAsync(Models.User user, Models.http.CheckItemCreateDTO form)
        {
            var item = await _todos.GetByIdAsync(user, form.todo_id, false);
            if (item == null) throw new NotFoundException();
            item.CheckItems.Add(form.ToDb());
            item.CheckItems.UpdateOrder();
            await _todos.SaveChangesAsync();
            return new Models.http.TodoDTO(item);
        }*/

        public async Task<Models.http.CheckItemDTO> UpdateAsync(Models.User user, Models.http.CheckItemDTO form)
        {
            var item = await _checkitems.ResolveAsync(user, form.id, form.syncId);
            if (item == null) throw new NotFoundException();
            if (form.title != null) item.Title = form.title;
            if (form.is_completed != null) item.IsCompleted = form.is_completed ?? false;
            await _checkitems.SaveChangesAsync();
            return new Models.http.CheckItemDTO(item);
        }

        public void UpdateList(Models.Todo todo, List<Models.http.CheckItemDTO> items)
        {
            var resolved = new List<(CheckItem Item, CheckItemDTO Input)>();
            var syncIds = new HashSet<Guid>();
            foreach (var incoming in items)
            {
                var existing = todo.CheckItems.FindIdentity(incoming.id, incoming.syncId);
                if (existing == null && incoming.id is > 0) throw new NotFoundException();
                var item = existing ?? new CheckItem
                {
                    SyncId = incoming.syncId ?? Guid.NewGuid(),
                    TodoId = todo.Id
                };
                if (!syncIds.Add(item.SyncId)) throw new InvalidRequestException("Duplicate checklist item.");
                resolved.Add((item, incoming));
            }

            var retained = resolved.Select(x => x.Item).ToHashSet();
            todo.CheckItems.RemoveAll(x => !retained.Contains(x));
            for (var i = 0; i < resolved.Count; i++)
            {
                var (item, incoming) = resolved[i];
                item.Title = incoming.title ?? "";
                item.IsCompleted = incoming.is_completed ?? false;
                item.Order = i;
                if (!todo.CheckItems.Contains(item)) todo.CheckItems.Add(item);
            }
            todo.CheckItems.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public async Task<Models.http.CheckItemDTO> ToggleAsync(Models.User user, long id, Guid? syncId = null)
        {
            var item = await _checkitems.ResolveAsync(user, id, syncId);
            if (item == null) throw new NotFoundException();
            item.IsCompleted = !item.IsCompleted;
            await _checkitems.SaveChangesAsync();
            return new Models.http.CheckItemDTO(item);
        }

        public async Task DeleteAsync(Models.User user, long id, Guid? syncId = null)
        {
            var item = await _checkitems.ResolveAsync(user, id, syncId);
            if (item == null) throw new NotFoundException();
            await _checkitems.RemoveAsync(item);
        }

        public async Task<Todo> MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            var item = await _checkitems.ResolveAsync(user, form.id, form.syncId, true);
            form.id = item.Id;
            if (form.before_id.HasValue || form.beforeSyncId.HasValue)
            {
                var before = await _checkitems.ResolveAsync(user, form.before_id, form.beforeSyncId, true);
                if (before.TodoId != item.TodoId) throw new InvalidRequestException("Checklist items must belong to the same task.");
                form.before_id = before.Id;
            }
            if (item == null) throw new NotFoundException();

            var todo = await _todos.GetByIdAsync(user, item.TodoId, false);
            if (todo == null) throw new NotFoundException();

            todo.CheckItems.MoveBefore(form.id, form.before_id);

            await _todos.SaveChangesAsync();
            return todo;
        }
    }
}
