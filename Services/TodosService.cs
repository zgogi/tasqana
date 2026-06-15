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
        public TodosService(
            TodosRepository todos,
            CheckItemService checkitems
            ) { 
            _todos = todos;
            _checkitems = checkitems;
        }

        public async Task<Models.Todo> InsertAsync(Models.User user, Models.http.TodoDTO form)
        {
            var other = await _todos.GetByCategoryAsync(user, form.category_id, true);
            var todo = form.ToTodo(user);
            todo.Order = other.Count;

            if (form.check_items != null)
            {
                _checkitems.UpdateList(todo, form.check_items.ToList());
            }

            var result = await _todos.InsertAsync(todo);
            return result;
        }

        public async Task<Models.Todo> UpdateAsync(Models.User user, Models.http.TodoDTO form)
        {
            if (!form.id.HasValue) throw new NotFoundException();
            var item = await _todos.GetByIdAsync(user, form.id ?? 0, false);
            if (item == null) { throw new NotFoundException(); }
            if (form.title != null) item.Title = form.title;
            if (form.description != null) item.Description = form.description;
            if (form.category_id != null) item.CategoryId = form.category_id;
            if (form.state != null) item.State = (Models.TodoState)form.state;
            if (form.priority != null) item.Priority = (Models.Priority)form.priority;
            
            if (form.check_items != null)
            {
                _checkitems.UpdateList(item, form.check_items.ToList());
            }

            await _todos.SaveChangesAsync();
            return item;
        }

        public async Task<IEnumerable<Models.http.TodoExtDTO>> GetAllAsync()
        {
            var result = await _todos.GetAllAsync(true);
            return result.Select(e => new Models.http.TodoExtDTO(e));
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

    }
}
