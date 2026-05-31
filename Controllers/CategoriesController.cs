using Microsoft.AspNetCore.Mvc;
using WebApi.Models.http;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController, Route("api/v1.0/categories")]
    public class CategoriesController : AbstractController
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly CategoriesService _categories;
        public CategoriesController(
            SessionsService sessions,
            ILogger<CategoriesController> logger,
            CategoriesService categories
            ):base(sessions)
        {
            _logger = logger;
            _categories = categories;
        }

        [HttpPost, Route("create")]
        public async Task<ActionResult> Create(Models.http.CategoryCreateDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _categories.InsertAsync(user, form.title, form.parent_id);
                return Ok(result);
            });
            
        }

        [HttpPost, Route("update")]
        public async Task<ActionResult> Update(Models.http.CategoryUpdateDTO form)
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _categories.UpdateAsync(user, form);
                return Ok(result);
            });

        }

        [HttpGet, Route("tree")]
        public async Task<ActionResult> GetTreeAsync()
        {
            return await WithAuthenticationAsync(async user => {
                var result = await _categories.GetTreeAsync(user);
                return Ok(result);
            });
        }

        [HttpPost, Route("delete")]
        public async Task<ActionResult> DeleteAsync(CategoryDeleteDTO form)
        {
            return await WithAuthenticationAsync(async user => { 
                await _categories.DeleteAsync(user, form.id);
                return NoContent();   
            });
        }

    }
}
