using Newtonsoft.Json;
using StackExchange.Redis;
using RabbitMQServer.Models;
using System.Text.RegularExpressions;
using System.Text;

namespace RabbitMQServer.Services
{
    public class AccountService
    {
        private readonly string LogFilePath = "./Data/auth_log.txt";
        private readonly string UserDataFilePath = "./Data/user_data.txt";
        private readonly JwtSettings jwtSettings;
        private readonly Logger logger;

        public AccountService(JwtSettings jwtSettings)
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
                    throw new Exception($"Invalid data format: {JsonConvert.SerializeObject(user)}");
                }

                // make user password hash
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Email = user.Email.ToLower();

                // if this user already exists
                var userExist = FindUser(user.Email);
                if (userExist != null)
                {
                    throw new Exception($"User alredy exists: {JsonConvert.SerializeObject(user)}");
                }

                // Saving user data to "db"
                SaveUserToDB(user);

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
                // compare users db data with user request data
                request.Email = request.Email.ToLower();
                var storedUser = FindUser(request.Email);
                if (storedUser == null)
                {
                    throw new Exception("User not find");
                }

                // if the user was found
                if (BCrypt.Net.BCrypt.Verify(request.Password, storedUser.Password))
                {
                    // create token
                    var Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        Email = storedUser.Email,
                        UserName = storedUser.UserName,
                    }, jwtSettings);

                    // Saving user data to Redis
                    RedisConnection(storedUser.Email, Token.Token);
                    logger.LogInfo($"Login: {JsonConvert.SerializeObject(Token)}");

                    return Token;
                }
                else
                {
                    throw new Exception("Wrong password");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Login error: {ex.Message}");
                return null;
            }
        }

        public bool DeleteUser(UserDto user)
        {
            try
            {
                // find user
                var ObjToRemove = FindUser(user.Email);
                if (ObjToRemove == null)
                {
                    throw new Exception($"User not find: {JsonConvert.SerializeObject(user)}");
                }
                var lineToRemove = JsonConvert.SerializeObject(ObjToRemove);
                Console.WriteLine(lineToRemove);

                string[] lines = File.ReadAllLines(UserDataFilePath);

                // create new file data
                var newLines = new StringBuilder();
                foreach (string line in lines)
                {
                    if (line.Normalize() != lineToRemove.Normalize())
                    {
                        newLines.AppendLine(line);
                    }
                }

                File.WriteAllText(UserDataFilePath, newLines.ToString());

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Delete error: {ex.Message}");
                return false;
            }
        }

        private void SaveUserToDB(Users user)
        {
            string json = JsonConvert.SerializeObject(user);
            using (var writer = new StreamWriter(UserDataFilePath, true))
            {
                writer.WriteLine(json);
            }

            logger.LogInfo($"Authorized: {json}");
        }

        private Users FindUser(string request)
        {
            string[] jsonLines = File.ReadAllLines(UserDataFilePath);

            // compare users db data with user request data
            var storedUser = new Users();
            foreach (string line in jsonLines)
            {
                storedUser = JsonConvert.DeserializeObject<Users>(line);

                if (storedUser != null && storedUser.Email == request)
                {
                    break;
                }
                storedUser = null;
            }

            return storedUser;
        }

        private static bool IsUserDataValid(Users user)
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