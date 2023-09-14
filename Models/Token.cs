using System.Security.Cryptography;

namespace RabbitMQServer.Models
{
    public class Token
    {
        private const int TokenLength = 32;

        public static string GenerateToken()
        {
            byte[] randomBytes = new byte[TokenLength];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public static bool VerifyToken(string tokenToVerify, string storedToken)
        {
            byte[] tokenBytes = Convert.FromBase64String(tokenToVerify);
            byte[] storedTokenBytes = Convert.FromBase64String(storedToken);

            if (tokenBytes.Length != TokenLength || storedTokenBytes.Length != TokenLength)
            {
                return false;
            }

            for (int i = 0; i < TokenLength; i++)
            {
                if (tokenBytes[i] != storedTokenBytes[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}