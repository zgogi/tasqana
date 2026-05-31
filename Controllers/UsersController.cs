using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApi.Models.http;
using WebApi.Repositories;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController, Route("api/v1.0/users")]
    public class UsersController : AbstractController
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UsersService _users;
        public UsersController(
            SessionsService sessions,
            ILogger<UsersController> logger,
            UsersService users):base(sessions)
        {
            _logger = logger;
            _users = users;
        }

        [HttpPost, Route("token/update")]
        public async Task<ActionResult> TokenUpdate()
        {
            var user = await AuthenticateAsync();
            if (user == null) { return Unauthorized(); }
            var result = await _sessions.CreateSessionAsync(user, HttpContext, 60*24*7);
            return Ok(result);
        }

        [HttpGet, Route("list")]
        public async Task<ActionResult> Get()
        {
            return await WithAuthenticationAsync(async user => { 
                
                var result = await _users.GetAllAsync();
                return Ok(result);
            });
            
        }
    }
}
