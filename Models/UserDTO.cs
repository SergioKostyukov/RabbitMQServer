namespace RabbitMQServer.Models
{
    // class that stores user information at the login stage
    public class UserDto
    {
        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
