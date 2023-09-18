using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    public class ConsumerController : ControllerBase
    {
        private readonly ConsumerService _consumerService;

        public ConsumerController(ConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        [HttpPost]
        public IActionResult ReceiveMessage()
        {
            _consumerService.ReceiveMessage();

            return Ok("Message received successfully");
        }
    }
}