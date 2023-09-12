using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;

/*
 Переробити README file
 Додати .sln до .gitignore
 Розібратись що за трабл з Consumer-ом
 
 Додати Redis
 Додати сервіс авторизації
 */

namespace RabbitMQServer.Services
{
    public class ConsumerService
    {
        private static string LogFilePath = "./consumer_log.txt";
        private static string ConfFilePath = "./local.json";
        private static IConfiguration Configuration { get; set; }

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

                ClearFileContent(LogFilePath);

                using (var connection = factory.CreateConnection())
                {
                    LogInfo("Connection established");

                    using (var channel = connection.CreateModel())
                    {
                        LogInfo("Channel created");

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
                LogError($"An error occurred: {ex.Message}");
            }
        }

        private void ProcessMessageAsync(IModel channel, BasicDeliverEventArgs args)
        {
            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                // var message = JsonConvert.DeserializeObject<Message>(json);

                LogInfo($"Received: {json}");

                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                LogError($"Error processing message: {ex.Message}");
            }
        }

        private static void LogInfo(string message)
        {
            LogMessage($"[INFO] {message}");
        }

        private static void LogError(string message)
        {
            LogMessage($"[ERROR] {message}");
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine(message);

            try
            {
                using (StreamWriter writer = File.AppendText(LogFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
        }

        static void ClearFileContent(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing file content: {ex.Message}");
            }
        }
    }
}
