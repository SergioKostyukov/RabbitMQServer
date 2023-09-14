namespace RabbitMQServer.Models
{
    // class that stores full user information
    public class User
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
