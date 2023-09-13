using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.AspNetCore.Connections;
using RabbitMQServer.Models;
using System.Threading.Channels;
using System.Text.RegularExpressions;

namespace RabbitMQServer.Services
{
    public class AuthService
    {
        //private static string ConfFilePath = "./Data/local.json";
        private static string LogFilePath = "./Data/auth_log.txt";
        private static string UserData = "./Data/user_data.txt";
        //private static IConfiguration Configuration { get; set; }

        public bool AuthUser(User user)
        {
            //RedisConnection();

            try
            {
                if (!IsEmailValid(user.Email))
                {
                    LogInfo($"Invalid Email format: {user.Email}");
                    return false;
                }

                user.Email = user.Email.ToLower();
                string json = JsonConvert.SerializeObject(user);

                using (StreamWriter writer = new StreamWriter(UserData, true))
                {
                    writer.WriteLine($"{json}");
                }

                LogInfo($"Received: {json}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }
        
        public bool LoginUser(UserDTO user)
        {
            try
            {
                user.Email = user.Email.ToLower();
                string json = JsonConvert.SerializeObject(user);

                string[] jsonLines;
                using (var streamReader = new StreamReader(UserData))
                {
                    jsonLines = streamReader.ReadToEnd().Split("\n");
                }

                User result_user = null;

                foreach (string line in jsonLines)
                {
                    
                    User storedUser = JsonConvert.DeserializeObject<User>(line);
                    Console.WriteLine($"CurrUser - {storedUser.Email}");

                    if (storedUser.Email == user.Email)
                    {
                        if(storedUser.Password == user.Password)
                        {
                            result_user = storedUser;
                            break;
                        }
                        else
                        {
                            throw new Exception("Invalid password");
                        }
                    }
                }

                if(result_user != null)
                {
                    LogInfo($"Login: {json}");
                    return true;
                }

                throw new Exception("Doesn`t find");
            }
            catch (Exception ex)
            {
                LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }

        private bool IsEmailValid(string email)
        {
            // Перевірка валідності пошти за допомогою регулярного виразу
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
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
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
        }

        /*private static void RedisConnection()
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

        public bool Authenticate(string username, string password)
        {
            // Отримати пароль з Redis за ключем, який відповідає імені користувача
            string storedPassword = _redisDatabase.StringGet(username);

            // Порівняти отриманий пароль із введеним паролем
            return password == storedPassword;
        }*/
    }
}