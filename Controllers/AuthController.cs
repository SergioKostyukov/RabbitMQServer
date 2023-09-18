using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;
using RabbitMQServer.Models;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Authorization")]
        public IActionResult Authorization(Users user)
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
            var userToken = _authService.LoginUser(user);
            if (userToken != null)
            {
                return Ok($"User login successfull:\n{userToken}");
            }
            else
            {
                return BadRequest("User login failed");
            }
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