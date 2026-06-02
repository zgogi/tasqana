using Microsoft.AspNetCore.Mvc;
using WebApi.Models.http;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController, Route("api/v1.0/dashboard")]
    public class DashboardController : AbstractController
    {
        private readonly UsersService _users;
        private readonly CategoriesService _categories;
        private readonly TodosService _todos;

        public DashboardController(
            UsersService users,
            SessionsService sessions,
            CategoriesService categories,
            TodosService todos
            ):base(sessions)
        {
            _users = users;
            _categories = categories;
            _todos = todos;
        }


     /*   [HttpGet, Route("home")]
        public async Task<ActionResult> Get()
        {
            return await WithAuthenticationAsync(async user => {

                var result = new DashboardDTO();
                result.categories = await _categories.GetTreeAsync(user);
                result.unsorted = (await _todos.GetByCategoryAsync(user, null))
                    .ConvertAll(item => new TodoDTO(item));
                return Ok(result);
            });

        }*/
    }
}
