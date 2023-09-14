using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.AspNetCore.Connections;
using RabbitMQServer.Models;
using System.Threading.Channels;
using System.Text.RegularExpressions;

/*
    Переробити README file
    Розібратись що за трабл з Consumer-ом
    Add comments
 
    Add Redis
    Add Authorization
 */

namespace RabbitMQServer.Services
{
    public class AuthService
    {
        //private readonly static string ConfFilePath = "./Data/local.json";
        private readonly string LogFilePath = "./Data/auth_log.txt";
        private readonly string UserDataFilePath = "./Data/user_data.txt";
        private readonly Logger logger;

        public AuthService()
        {
            logger = new Logger(LogFilePath);
        }

        public bool AuthUser(User user)
        {
            try
            {
                if (!IsEmailValid(user.Email))
                {
                    logger.LogInfo($"Invalid Email format: {user.Email}");
                    return false;
                } // add username and password validation

                // Generate token for the user's password


                // Saving user data to "db"
                user.Email = user.Email.ToLower();
                string json = JsonConvert.SerializeObject(user);
                using (StreamWriter writer = new StreamWriter(UserDataFilePath, true))
                {
                    writer.WriteLine($"{json}");
                }

                logger.LogInfo($"Received: {json}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }
        
        public bool LoginUser(UserDTO user)
        {
            try
            {
                user.Email = user.Email.ToLower();

                string[] jsonLines = File.ReadAllLines(UserDataFilePath);

                foreach (string line in jsonLines)
                {
                    User storedUser = JsonConvert.DeserializeObject<User>(line);

                    if (storedUser != null && storedUser.Email == user.Email)
                    {
                        if (VerifyPassword(storedUser.Password, user.Password))
                        {
                            logger.LogInfo($"Login: {JsonConvert.SerializeObject(user)}");
                            return true;
                        }
                        else
                        {
                            throw new Exception("Invalid password");
                        }
                    }
                }

                throw new Exception("Doesn`t find");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }

        private bool VerifyPassword(string storedHash, string inputPassword)
        {
            if(storedHash == inputPassword)
            {
                return true;
            }

            return false;
        }

        private bool IsEmailValid(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
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
        }*/
    }
}