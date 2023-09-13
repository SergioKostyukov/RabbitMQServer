using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;
using RabbitMQServer.Models;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Authorization")]
        public IActionResult Authorization(User user)
        {
            if (_authService.AuthUser(user))
            {
                return Ok("User authorization successful");
            }
            else
            {
                return BadRequest("User authorization failed");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(UserDTO user)
        {
            if (_authService.LoginUser(user))
            {
                return Ok("User login successfull");
            }
            else
            {
                return BadRequest("User login failed");
            }
        }
    }
}