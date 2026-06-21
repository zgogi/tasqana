using Microsoft.AspNetCore.Mvc;
using Tasqana.Models;
using Tasqana.Services;
using Tasqana.Extensions;
using Tasqana.Models.http;

namespace Tasqana.Controllers
{
    [ApiController, Route("api/v1.0/todos")]
    public class TodosController : AbstractController
    {
        private readonly TodosService _todos;
        private readonly CheckItemService _checkItems;
        private readonly IFileStorageService _files;

        public TodosController(
            SessionsService sessions,
            TodosService todos,
            CheckItemService checkItems,
            IFileStorageService files
            ) : base(sessions)
        {
            _todos = todos;
            _checkItems = checkItems;
            _files = files;
        }

        [HttpPost, Route("create/form")]
        public async Task<ActionResult> Create([FromForm] Models.http.TodoDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var todo = await _todos.InsertAsync(user, form);
                return Ok(new Models.http.TodoDTO(todo, e => _files.GetUrl(e)));
            });
        }

      /*  [HttpPost, Route("update")]
        public async Task<ActionResult> Update(Models.http.TodoDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var todo = await _todos.UpdateAsync(user, form);
                return Ok(new Models.http.TodoDTO(todo, e => _files.GetUrl(e)));
            });
        }*/

        [HttpPost, Route("update/form")]
        public async Task<ActionResult> UpdateForm([FromForm] Models.http.TodoDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var todo = await _todos.UpdateAsync(user, form);
                return Ok(new Models.http.TodoDTO(todo, e => _files.GetUrl(e)));
            });
        }

        [HttpPost, Route("delete")]
        public async Task<ActionResult> Delete(Models.http.DeleteDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                await _todos.DeleteAsync(user, form.id);
                return NoContent();
            });
        }

        [HttpPost, Route("move")]
        public async Task<ActionResult> MoveAsync(Models.http.ReorderDTO form)
        {
            return await WithAuthenticationAsync(async user =>
            {
                var result = await _todos.MoveAsync(user, form);
                return Ok(result.Select(e => new TodoDTO(e, f => _files.GetUrl(f))));
            });
        }

        [HttpGet, Route("list")]
        public async Task<ActionResult> Get()
        {
            return await WithAuthenticationAsync(async user => {

                var categoryId = ReadQueryLong("category_id");
                var priority = ReadQueryBool("priority");
                var state = ReadQueryInt("state");
                var todos = await _todos.GetFilteredAsync(user, categoryId, priority == true, state.ToEnum<TodoState>());
                var tododtos = todos.Select(e => new Models.http.TodoDTO(e, f => _files.GetUrl(f)));
                return Ok(tododtos);
            });

        }

        
    }
}
