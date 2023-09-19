using System.ComponentModel.DataAnnotations;

namespace RabbitMQServer.Models
{
    // class that stores user information at the login stage
    public class UserDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
