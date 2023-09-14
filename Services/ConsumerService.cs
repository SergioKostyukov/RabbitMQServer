using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Fix an error in the Consumer 

namespace RabbitMQServer.Services
{
    public class ConsumerService
    {
        private readonly static string LogFilePath = "./Data/consumer_log.txt";
        private readonly static string ConfFilePath = "./Data/local.json";
        private static IConfiguration Configuration { get; set; }
        private readonly Logger logger;
        public ConsumerService()
        {
            logger = new Logger(LogFilePath);
        }

        public void ReceiveMessage()
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
                    ClientProvidedName = "Rabbit Consumer"
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

                        var consumer = new EventingBasicConsumer(channel);

                        consumer.Received += (sender, eventArgs) =>
                        {
                            //Task.Delay(TimeSpan.FromSeconds(5));

                            ProcessMessageAsync(channel, eventArgs);
                        };

                        string consumerTag = channel.BasicConsume(queueName, autoAck: false, consumer: consumer);

                        channel.BasicCancel(consumerTag);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
        }

        private void ProcessMessageAsync(IModel channel, BasicDeliverEventArgs args)
        {
            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                // var message = JsonConvert.DeserializeObject<Message>(json);

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
