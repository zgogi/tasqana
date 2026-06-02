using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController, Route("api/v1.0/todos")]
    public class TodosController : AbstractController
    {
        private readonly TodosService _todos;
        private readonly CheckItemService _checkItems;

        public TodosController(
            SessionsService sessions,
            TodosService todos,
            CheckItemService checkItems
            ) : base(sessions)
        {
            _todos = todos;
            _checkItems = checkItems;
        }

        [HttpPost, Route("create")]
        public async Task<ActionResult> Create(Models.http.TodoCreateDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var todo = await _todos.InsertAsync(user, form);
                return Ok(todo);
            });
        }

        [HttpPost, Route("update")]
        public async Task<ActionResult> Update(Models.http.TodoDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var todo = await _todos.UpdateAsync(user, form);
                return Ok(todo);
            });
        }

        [HttpPost, Route("delete")]
        public async Task<ActionResult> Delete(Models.http.TodoDeleteDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                await _todos.DeleteAsync(user, form.id);
                return NoContent();
            });
        }

        [HttpGet, Route("list")]
        public async Task<ActionResult> Get()
        {
            return await WithAuthenticationAsync(async user => {

                var categoryId = ReadQueryLong("category_id");
                var result = await _todos.GetByCategoryAsync(user, categoryId);
                return Ok(result);
            });

        }

        
    }
}
