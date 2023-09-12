using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.AspNetCore.Connections;

namespace RabbitMQServer.Services
{
    public class ProducerService
    {
        private static string LogFilePath = "./producer_log.txt";
        private static string ConfFilePath = "./local.json";
        private static IConfiguration Configuration { get; set; }

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


                //RedisConnection();


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
                                LogInfo($"Sent: {json}");
                            }
                            catch (Exception ex)
                            {
                                LogError($"Error sending message: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"An error occurred: {ex.Message}");
            }
        }

        private static void RedisConnection()
        {
            string redisConnectionString = "localhost:6379";

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase db = redis.GetDatabase();

            // Збереження значення в Redis
            db.StringSet("mykey", "Hello, Redis!");

            // Отримання значення з Redis
            string value = db.StringGet("mykey");
            Console.WriteLine(value);
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

        private static void ClearFileContent(string filePath)
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