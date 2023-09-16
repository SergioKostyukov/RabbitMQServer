using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQServer.Services
{
    public class ConsumerService
    {
        private readonly static string LogFilePath = "./Data/consumer_log.txt";
        private readonly static string ConfFilePath = "./Data/local.json";
        private static IConfiguration? Configuration { get; set; }
        private readonly Logger logger;
        public ConsumerService()
        {
            logger = new Logger(LogFilePath);
            //logger.ClearFileContent();
        }

        public void ReceiveMessage()
        {
            try
            {
                var factory = CreateFactory();

                using var connection = factory.CreateConnection();
                logger.LogInfo("Connection established");

                using var channel = connection.CreateModel();
                logger.LogInfo("Channel created");

                // Set queue parameters 
                string exchangeName = "Exchange";
                string routingKey = "routing-key";
                string queueName = "Queue";

                // Description of exchange and queue
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queueName, exchangeName, routingKey);

                // Create consumer to process messages 
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, eventArgs) =>
                {
                    ProcessMessage(channel, eventArgs);
                };

                // Consumer unsubscribing 
                string consumerTag = channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
                channel.BasicCancel(consumerTag);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
        }

        private static ConnectionFactory CreateFactory()
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
                ClientProvidedName = "Rabbit Consumer"
            };

            return factory;
        }

        private void ProcessMessage(IModel channel, BasicDeliverEventArgs args)
        {
            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                logger.LogInfo($"Received: {json}");

                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing message: {ex.Message}");
            }
        }
    }
}
