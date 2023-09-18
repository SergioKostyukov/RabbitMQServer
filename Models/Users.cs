using Newtonsoft.Json;

namespace RabbitMQServer.Models
{
    // class that stores full user information
    public class Users
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
