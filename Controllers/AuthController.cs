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
        public IActionResult Login(UserDto user)
        {
            if (!_authService.LoginUser(user))
            {
                return BadRequest("User login failed"); 
            }

            if (!_authService.CreateToken(user))
            {
                return BadRequest("Token creation error");
            }

            return Ok("User login successfull");
        }

        [HttpPost("Delete")]
        public IActionResult Delete(UserDto user)
        {
            if (true)
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