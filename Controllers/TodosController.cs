using Microsoft.AspNetCore.Mvc;
using Tasqana.Models;
using Tasqana.Services;
using Tasqana.Extensions;

namespace Tasqana.Controllers
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
                var priority = ReadQueryBool("priority");
                var state = ReadQueryInt("state");
                var result = await _todos.GetFilteredAsync(user, categoryId, priority == true, state.ToEnum<TodoState>());
                return Ok(result);
            });

        }

        
    }
}
