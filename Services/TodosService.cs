using Microsoft.EntityFrameworkCore;
using Tasqana.Extensions;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class TodosService
    {
        private readonly TodosRepository _todos;
        public TodosService(
            TodosRepository todos
            ) { 
            _todos = todos;
        }

        public async Task<Models.Todo> InsertAsync(Models.User user, long? categoryId, string title, string? description)
        {
            var todo = new Models.Todo { 
                UserId = user.Id,
                CategoryId = categoryId,
                Title = title, 
                Description = description
            };
            return await _todos.InsertAsync( todo );
        }

        public async Task<Models.Todo> InsertAsync(Models.User user, Models.http.TodoCreateDTO form)
        {
            var todo = form.ToTodo(user);
            return await _todos.InsertAsync(todo);
        }

        public async Task<Models.http.TodoDTO> UpdateAsync(Models.User user, Models.http.TodoDTO source)
        {
            var item = await _todos.GetByIdAsync(user, source.id, false);
            if (item == null) { throw new NotFoundException(); }
            if (source.title != null) item.Title = source.title;
            if (source.description != null) item.Description = source.description;
            if (source.category_id != null) item.CategoryId = source.category_id;
            if (source.state != null) item.State = (Models.TodoState)source.state;
            if (source.priority != null) item.Priority = (Models.Priority)source.priority;
            await _todos.SaveChangesAsync();
            return new Models.http.TodoDTO(item);
        }

        public async Task<IEnumerable<Models.http.TodoExtDTO>> GetAllAsync()
        {
            var result = await _todos.GetAllAsync(true);
            return result.Select(e => new Models.http.TodoExtDTO(e));
        }

        public async Task<IEnumerable<Models.http.TodoDTO>> GetFilteredAsync(Models.User user, long? categoryId, bool priority, Models.TodoState? state)
        {
            var result = new List<Models.Todo>();
            if (priority)
                result = await _todos.GetByPriorityAsync(user, Models.Priority.Low);
            else if (state != null)
                result = await _todos.GetByStateAsync(user, state ?? Models.TodoState.Completed);
            else
                result = await _todos.GetByCategoryAsync(user, categoryId);
            return result.Select(e => new Models.http.TodoDTO(e));
        }

        public async Task DeleteAsync(Models.User user, long id)
        {
            await _todos.DeleteAsync(user, id);
        }

        public async Task<IEnumerable<Models.http.TodoDTO>> MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            var item = await _todos.GetByIdAsync(user, form.id, true);
            if (item == null) throw new NotFoundException();

            var items = await _todos.GetByCategoryAsync(user, item.CategoryId, false);
            items.MoveBefore(form.id, form.before_id);
            await _todos.SaveChangesAsync();
            return items.Select(e => new Models.http.TodoDTO(e));
        }

    }
}
