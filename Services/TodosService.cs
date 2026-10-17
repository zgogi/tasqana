using Amazon.S3.Model.Internal.MarshallTransformations;
using Microsoft.EntityFrameworkCore;
using Tasqana.Extensions;
using Tasqana.Models;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class TodosService
    {
        private readonly TodosRepository _todos;
        private readonly CheckItemService _checkitems;
        private readonly TodoMediaService _media;
        public TodosService(
            TodosRepository todos,
            CheckItemService checkitems,
            TodoMediaService media
            ) { 
            _todos = todos;
            _checkitems = checkitems;
            _media = media;
        }

        public async Task<Models.Todo> InsertAsync(Models.User user, Models.http.TodoDTO form)
        {
            SyncIdentity.Validate(form.syncId);
            if (form.syncId.HasValue)
            {
                var existing = await _todos.Query(false).Include(c => c.CheckItems).Include(c => c.Files)
                    .SingleOrDefaultAsync(c => c.UserId == user.Id && c.SyncId == form.syncId);
                if (existing != null) return existing;
            }
            await ResolveCategoryAsync(user, form);
            var other = await _todos.GetByCategoryAsync(user, form.category_id, true);
            var todo = form.ToTodo(user);
            todo.Order = other.Count;
            UpdateItem(todo, form);
            var result = await _todos.InsertAsync(todo);
            if (form.files != null)
            {
                var count = form.files.Count(e => true);
                if (count > 0)
                {
                    await UpdateItemFilesAsync(result, form);
                    await _todos.SaveChangesAsync();
                }
            }
            return await _todos.ResolveAsync(user, result.Id, null);
        }

        public async Task<Models.Todo> UpdateAsync(Models.User user, Models.http.TodoDTO form)
        {
            var item = await _todos.ResolveAsync(user, form.id, form.syncId);
            await ResolveCategoryAsync(user, form);
            if (item == null) { throw new NotFoundException(); }
            UpdateItem(item, form);
            await UpdateItemFilesAsync(item, form);
            await _todos.SaveChangesAsync();
            return item;
        }

        public async Task<IEnumerable<Models.Todo>> GetAllAsync()
        {
            var result = await _todos.GetAllAsync(true);
            return result;
        }

        public async Task<IEnumerable<Models.Todo>> GetFilteredAsync(Models.User user, long? categoryId, bool priority, Models.TodoState? state, Guid? categorySyncId = null)
        {
            if (categorySyncId.HasValue)
                categoryId = (await _todos.ResolveCategoryAsync(user, categoryId, categorySyncId)).Id;
            var result = new List<Models.Todo>();
            if (priority)
                result = await _todos.GetByPriorityAsync(user, Models.Priority.Low);
            else if (state != null)
                result = await _todos.GetByStateAsync(user, state ?? Models.TodoState.Completed);
            else
                result = await _todos.GetByCategoryAsync(user, categoryId);
            return result;
        }

        public async Task DeleteAsync(Models.User user, long id, Guid? syncId = null)
        {
            var item = await _todos.ResolveAsync(user, id, syncId);
            await _todos.RemoveAsync(item);
        }

        public async Task<IEnumerable<Models.Todo>> MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            var item = await _todos.ResolveAsync(user, form.id, form.syncId, true);
            form.id = item.Id;
            if (form.before_id.HasValue || form.beforeSyncId.HasValue)
            {
                var before = await _todos.ResolveAsync(user, form.before_id, form.beforeSyncId, true);
                if (before.CategoryId != item.CategoryId) throw new InvalidRequestException("Tasks must belong to the same category.");
                form.before_id = before.Id;
            }
            if (item == null) throw new NotFoundException();

            var items = await _todos.GetByCategoryAsync(user, item.CategoryId, false);
            items.MoveBefore(form.id, form.before_id);
          
            await _todos.SaveChangesAsync();
            return items;
        }

        private async Task ResolveCategoryAsync(Models.User user, Models.http.TodoDTO form)
        {
            if (form.category_id.HasValue || form.categorySyncId.HasValue)
            {
                var category = await _todos.ResolveCategoryAsync(user, form.category_id, form.categorySyncId);
                form.category_id = category.Id;
            }
        }

        private void UpdateItem(Models.Todo todo, Models.http.TodoDTO form)
        {
            if (form.title != null) todo.Title = form.title;
            if (form.description != null) todo.Description = form.description;
            if (form.category_id != null) todo.CategoryId = form.category_id;
            if (form.state != null) todo.State = (Models.TodoState)form.state;
            if (form.priority != null) todo.Priority = (Models.Priority)form.priority;

            if (form.check_items != null)
            {
                _checkitems.UpdateList(todo, form.check_items.ToList());
            }
        }

        private async Task UpdateItemFilesAsync(Models.Todo todo, Models.http.TodoDTO form)
        {
            foreach (var file in form.files ?? [])
            {
                var existing = todo.Files.FindIdentity(file.id, file.syncId);
                if (file.id is > 0 && existing == null) throw new NotFoundException();
                if (file.is_deleted)
                {
                    var itemToRemove = existing;
                    if (itemToRemove == null) continue;
                    todo.Files.Remove(itemToRemove);
                }
                else if (file.content != null && existing == null)
                {
                    using var content = file.content.OpenReadStream();
                    var newFile = await _media.InsertAsync(todo, file.content.FileName, content, syncId: file.syncId);
                    todo.Files.Add(newFile);
                }
            }
        }

    }
}
