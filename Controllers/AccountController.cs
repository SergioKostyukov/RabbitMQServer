using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;
using RabbitMQServer.Models;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _authService;
        
        public AccountController(AccountService authService)
        {
            _authService = authService;
        }

        [HttpPost]
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

        [HttpPost]
        public IActionResult Login(UserDto user)
        {
            var userToken = _authService.LoginUser(user);

            if (userToken != null)
            {
                return Ok(userToken);
            }
            else
            {
                return BadRequest("User login failed");
            }
        }

        [HttpPost]
        public IActionResult Delete(UserDto user)
        {
            if (_authService.DeleteUser(user))
            {
                return Ok("User delete successfull");
            }
            else
            {
                return BadRequest("User delete failed");
            }
        }
    }
}