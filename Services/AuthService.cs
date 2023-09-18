using Newtonsoft.Json;
using StackExchange.Redis;
using RabbitMQServer.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace RabbitMQServer.Services
{
    public class AuthService
    {
        // private readonly static string ConfFilePath = "./Data/local.json";
        private readonly string LogFilePath = "./Data/auth_log.txt";
        private readonly string UserDataFilePath = "./Data/user_data.txt";
        private readonly JwtSettings jwtSettings;
        private readonly Logger logger;

        public AuthService(JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
            logger = new Logger(LogFilePath);
            //logger.ClearFileContent();
        }

        public bool AuthUser(Users user)
        {
            try
            {
                // user data validation
                if (!IsUserDataValid(user))
                {
                    logger.LogInfo($"Invalid data format: {user}");
                    return false;
                } 

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
        
        public UserTokens? LoginUser(UserDto request)
        {
            try
            {
                string[] jsonLines = File.ReadAllLines(UserDataFilePath);

                // compare users db data with user request data
                var storedUser = new Users();
                foreach (string line in jsonLines)
                {
                    storedUser = JsonConvert.DeserializeObject<Users>(line);

                    if (storedUser != null && storedUser.UserName == request.UserName)
                    {
                        break;
                    }
                    storedUser = null;
                }

                // if the user was found
                var Token = new UserTokens();
                if (storedUser != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(request.Password, storedUser.Password))
                    {
                        // create token
                        Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                        {
                            Email = storedUser.Email,
                            UserName = storedUser.UserName,
                        }, jwtSettings);

                        // Saving user data to Redis
                        RedisConnection(storedUser.Email, Token.Token);

                        logger.LogInfo($"Login: {JsonConvert.SerializeObject(Token)}");
                    }
                    else
                    {
                        throw new Exception("Wrong password");
                    }
                }
                else
                {
                    throw new Exception("User not find");
                }

                return Token;
            }
            catch (Exception ex)
            {
                logger.LogError($"Login error: {ex.Message}");
                return null;
            }
        }

        private static bool IsUserDataValid(Users user)// + username and password validation
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(user.Email, emailPattern);
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