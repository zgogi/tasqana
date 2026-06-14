using Microsoft.AspNetCore.Mvc;

namespace Tasqana.Controllers
{
    [ApiController]
    [Route("api/v1.0")]
    public class TestContoller : ControllerBase
    {
        [HttpGet, Route("test")]
        public ActionResult Test()
        {
            return Ok();
        }
    }
}
