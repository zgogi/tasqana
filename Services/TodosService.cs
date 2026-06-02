using Microsoft.EntityFrameworkCore;
using WebApi.Extensions;
using WebApi.Repositories;

namespace WebApi.Services
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
            if (item == null) { throw new Exception("Todo item not found"); }
            if (source.title != null) item.Title = source.title;
            if (source.description != null) item.Description = source.description;
            if (source.category_id != null) item.CategoryId = source.category_id;
            if (source.state != null) item.State = (Models.TodoState)source.state;
            await _todos.SaveChangesAsync();
            return new Models.http.TodoDTO(item);
        }

        public async Task<List<Models.Todo>> GetAllAsync()
        {
            return await _todos
                .Query(true)
                .Include(c => c.User)
                .Include(c => c.Category)
                .ToListAsync();
        }

       // public async Task<Models.Todo> Insert(Models.User user, long id)
       // {
       //     return await _todos.GetByIdAsync(user, id);
       // }
        public async Task<IEnumerable<Models.http.TodoDTO>> GetByCategoryAsync(Models.User user, long? categoryId)
        {
            var result = await _todos.GetByCategoryAsync(user, categoryId);
            return result.Select(e => new Models.http.TodoDTO(e));
        }

        public async Task DeleteAsync(Models.User user, long id)
        {
            await _todos.DeleteAsync(user, id);
        }

    }
}
