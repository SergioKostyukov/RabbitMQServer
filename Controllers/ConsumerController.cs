using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        private readonly ConsumerService _consumerService;

        public ConsumerController(ConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        [HttpPost("Receive")]
        public IActionResult ReceiveMessage()
        {
            _consumerService.ReceiveMessage();

            return Ok("Message received successfully");
        }
    }

}