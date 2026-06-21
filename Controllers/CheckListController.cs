using Microsoft.AspNetCore.Mvc;
using Tasqana.Controllers;
using Tasqana.Services;

namespace Tasqana.Controllers
{
    [ApiController, Route("api/v1.0/todos/checklist")]
    public class CheckListController : AbstractController
    {
        private readonly TodosService _todos;
        private readonly CheckItemService _checkItems;
        private readonly IFileStorageService _files;

        public CheckListController(
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

     /*   [HttpPost, Route("create")]
        public async Task<ActionResult> ChecklistItemCreate(Models.http.CheckItemCreateDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _checkItems.CreateAsync(user, form);
                return Ok(result);
            });
        }

        [HttpPost, Route("update")]
        public async Task<ActionResult> ChecklistItemUpdate(Models.http.CheckItemDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _checkItems.UpdateAsync(user, form);
                return Ok(result);
            });
        }

        [HttpPost, Route("delete")]
        public async Task<ActionResult> ChecklistItemDelete(Models.http.DeleteDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                await _checkItems.DeleteAsync(user, form.id);
                return Ok();
            });
        }*/

        [HttpPost, Route("move")]
        public async Task<ActionResult> ChecklistItemMove(Models.http.ReorderDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _checkItems.MoveAsync(user, form);
                return Ok(new Models.http.TodoDTO(result, e => _files.GetUrl(e)));
            });
        }

        [HttpPost, Route("toggle")]
        public async Task<ActionResult> ChecklistItemToggle(Models.http.DeleteDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _checkItems.ToggleAsync(user, form.id);
                return Ok(result);
            });
        }
    }
}
