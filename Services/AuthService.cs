using Newtonsoft.Json;
using StackExchange.Redis;
using RabbitMQServer.Models;
using System.Text.RegularExpressions;

namespace RabbitMQServer.Services
{
    public class AuthService
    {
        // private readonly static string ConfFilePath = "./Data/local.json";
        private readonly string LogFilePath = "./Data/auth_log.txt";
        private readonly string UserDataFilePath = "./Data/user_data.txt";
        private readonly Logger logger;

        public AuthService()
        {
            logger = new Logger(LogFilePath);
            logger.ClearFileContent();
        }

        public bool AuthUser(User user)
        {
            try
            {
                // user data validation
                if (user.Email != null && !IsEmailValid(user.Email))
                {
                    logger.LogInfo($"Invalid Email format: {user.Email}");
                    return false;
                } // + username and password validation

                // make user password hash
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                // Saving user data to "db"
                user.Email = user.Email.ToLower();
                string json = JsonConvert.SerializeObject(user);
                using (var writer = new StreamWriter(UserDataFilePath, true))
                {
                    writer.WriteLine(json);
                }

                logger.LogInfo($"Authorized: {json}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Autherization error: {ex.Message}");
                return false;
            }
        }
        
        public bool LoginUser(UserDto request)
        {
            try
            {
                request.Email = request.Email.ToLower();
                string[] jsonLines = File.ReadAllLines(UserDataFilePath);

                // compare users db data with user request data
                foreach (string line in jsonLines)
                {
                    User storedUser = JsonConvert.DeserializeObject<User>(line);

                    if (storedUser != null && storedUser.Email == request.Email)
                    {
                        if (BCrypt.Net.BCrypt.Verify(request.Password, storedUser.Password))
                        {
                            logger.LogInfo($"Login: {JsonConvert.SerializeObject(request)}");
                            return true;
                        }
                        else
                        {
                            throw new Exception("Wrong password");
                        }
                    }
                }

                throw new Exception("User not find");
            }
            catch (Exception ex)
            {
                logger.LogError($"Login error: {ex.Message}");
                return false;
            }
        }

        public bool CreateToken(UserDto user)
        {
            // Generate token for the user's password
            string token = Token.GenerateJWTToken(user);

            // Saving user data to Redis
            RedisConnection(user.Email, token);

            return true;
        }

        private static bool IsEmailValid(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }    

        private static void RedisConnection(string email, string key)
        {
            string redisConnectionString = "localhost:6379";

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase db = redis.GetDatabase();

            db.StringSet(email, key);

            // Correctness check
            string? value = db.StringGet(email);
            Console.WriteLine(value);
        }
    }
}