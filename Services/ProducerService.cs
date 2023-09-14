using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQServer.Models;

namespace RabbitMQServer.Services
{
    public class ProducerService
    {
        private readonly static string LogFilePath = "./Data/producer_log.txt";
        private readonly static string ConfFilePath = "./Data/local.json";
        private static IConfiguration Configuration { get; set; }
        private readonly Logger logger;

        public ProducerService()
        {
            logger = new Logger(LogFilePath);
        }

        public void SendMessage()
        {
            try
            {
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(ConfFilePath)
                .Build();

                var factory = new ConnectionFactory
                {
                    HostName = Configuration["RabbitMQ:HostName"],
                    Port = int.Parse(Configuration["RabbitMQ:Port"]),
                    UserName = Configuration["RabbitMQ:UserName"],
                    Password = Configuration["RabbitMQ:Password"],
                    ClientProvidedName = "Rabbit Producer"
                };

                logger.ClearFileContent();

                using (var connection = factory.CreateConnection())
                {
                    logger.LogInfo("Connection established");

                    using (var channel = connection.CreateModel())
                    {
                        logger.LogInfo("Channel created");

                        string exchangeName = "Exchange";
                        string routingKey = "routing-key";
                        string queueName = "Queue";

                        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                        channel.QueueBind(queueName, exchangeName, routingKey);

                        var messages = new[]
                        {
                            new Message { Content = "Message 1" },
                            new Message { Content = "Message 2" },
                            new Message { Content = "Message 3" }
                        };

                        foreach (var message in messages)
                        {
                            try
                            {
                                //Task.Delay(TimeSpan.FromSeconds(2));

                                var json = JsonConvert.SerializeObject(message);
                                var body = Encoding.UTF8.GetBytes(json);

                                channel.BasicPublish(exchangeName, routingKey, basicProperties: null, body: body);
                                logger.LogInfo($"Sent: {json}");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error sending message: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
        }
    }
}