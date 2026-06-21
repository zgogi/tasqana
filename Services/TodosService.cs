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
            return result;
        }

        public async Task<Models.Todo> UpdateAsync(Models.User user, Models.http.TodoDTO form)
        {
            if (!form.id.HasValue) throw new NotFoundException();
            var item = await _todos.GetByIdAsync(user, form.id ?? 0, false);
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

        public async Task<IEnumerable<Models.Todo>> GetFilteredAsync(Models.User user, long? categoryId, bool priority, Models.TodoState? state)
        {
            var result = new List<Models.Todo>();
            if (priority)
                result = await _todos.GetByPriorityAsync(user, Models.Priority.Low);
            else if (state != null)
                result = await _todos.GetByStateAsync(user, state ?? Models.TodoState.Completed);
            else
                result = await _todos.GetByCategoryAsync(user, categoryId);
            return result;
        }

        public async Task DeleteAsync(Models.User user, long id)
        {
            await _todos.DeleteAsync(user, id);
        }

        public async Task<IEnumerable<Models.Todo>> MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            var item = await _todos.GetByIdAsync(user, form.id, true);
            if (item == null) throw new NotFoundException();

            var items = await _todos.GetByCategoryAsync(user, item.CategoryId, false);
            items.MoveBefore(form.id, form.before_id);
          
            await _todos.SaveChangesAsync();
            return items;
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
            foreach (var file in form.files)
            {
                if (file.is_deleted)
                {
                    var itemToRemove = todo.Files.SingleOrDefault(e => e.Id == file.id);
                    if (itemToRemove == null) continue;
                    todo.Files.Remove(itemToRemove);
                }
                else if (file.content != null)
                {
                    using var content = file.content.OpenReadStream();
                    var newFile = await _media.InsertAsync(todo, file.content.FileName, content);
                    todo.Files.Add(newFile);
                }
            }
        }

    }
}
