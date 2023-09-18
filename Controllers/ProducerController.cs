using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQServer.Services;

namespace RabbitMQServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    public class ProducerController : ControllerBase
    {
        private readonly ProducerService _producerService;

        public ProducerController(ProducerService producerService)
        {
            _producerService = producerService;
        }

        [HttpPost("Send")]
        public IActionResult SendMessage()
        {
            _producerService.SendMessage();

            return Ok("Message sent successfully");
        }
    }

}