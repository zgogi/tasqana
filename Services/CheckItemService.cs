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
            var item = await _checkitems.GetByIdAsync(user, form.id ?? 0, false);
            if (item == null) throw new NotFoundException();
            if (form.title != null) item.Title = form.title;
            if (form.is_completed != null) item.IsCompleted = form.is_completed ?? false;
            await _checkitems.SaveChangesAsync();
            return new Models.http.CheckItemDTO(item);
        }

        public void UpdateList(Models.Todo todo, List<Models.http.CheckItemDTO> items)
        {
            var incomingIds = items
                 .Where(x => x.id.HasValue)
                 .Select(x => x.id!.Value)
                 .ToList();

            var itemsToRemove = todo.CheckItems
                .Where(existing => !incomingIds.Contains(existing.Id))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                todo.CheckItems.Remove(item);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var incomingItem = items[i];

                if (incomingItem.id.HasValue && incomingItem.id.Value > 0)
                {
                    var existingItem = todo.CheckItems
                        .FirstOrDefault(x => x.Id == incomingItem.id.Value);

                    if (existingItem != null)
                    {
                        existingItem.Title = incomingItem.title ?? "";
                        existingItem.IsCompleted = incomingItem.is_completed ?? false;
                        existingItem.Order = i; 
                    }
                }
                else
                {
                    var newItem = new CheckItem
                    {
                        Title = incomingItem.title ?? "",
                        IsCompleted = incomingItem.is_completed ?? false,
                        TodoId = todo.Id,
                        Order = i 
                    };

                    todo.CheckItems.Add(newItem);
                }
            }
            todo.CheckItems.Sort((a,b) => a.Order.CompareTo(b.Order));
        }

        public async Task<Models.http.CheckItemDTO> ToggleAsync(Models.User user, long id)
        {
            var item = await _checkitems.GetByIdAsync(user, id, false);
            if (item == null) throw new NotFoundException();
            item.IsCompleted = !item.IsCompleted;
            await _checkitems.SaveChangesAsync();
            return new Models.http.CheckItemDTO(item);
        }

        public async Task DeleteAsync(Models.User user, long id)
        {
            var item = await _checkitems.GetByIdAsync(user, id, false);
            if (item == null) throw new NotFoundException();
            await _checkitems.RemoveAsync(item);
        }

        public async Task<Todo> MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            var item = await _checkitems.GetByIdAsync(user, form.id, true);
            if (item == null) throw new NotFoundException();

            var todo = await _todos.GetByIdAsync(user, item.TodoId, false);
            if (todo == null) throw new NotFoundException();

            todo.CheckItems.MoveBefore(form.id, form.before_id);

            await _todos.SaveChangesAsync();
            return todo;
        }
    }
}
