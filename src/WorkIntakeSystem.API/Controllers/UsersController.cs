using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly GraphServiceClient _graphClient;
        public UsersController(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var user = new
            {
                Name = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            };
            return Ok(user);
        }

        [HttpGet("sync-azure-ad")]
        [Authorize(Roles = "SystemAdministrator")]
        public async Task<IActionResult> SyncAzureAdUsers()
        {
            var users = await _graphClient.Users.Request().Select("id,displayName,mail,userPrincipalName").GetAsync();
            return Ok(users.CurrentPage.Select(u => new { u.Id, u.DisplayName, u.Mail, u.UserPrincipalName }));
        }
    }
} 