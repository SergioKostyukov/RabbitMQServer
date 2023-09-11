using System;

namespace RabbitMQServer
{
    public class Message
    {
        public string Content { get; set; }
    }

    public class Response
    {
        public bool Approved { get; set; }
        public string Message { get; set; }
    }
}