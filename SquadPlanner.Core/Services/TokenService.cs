using FootballSquad.Core.ServiceContracts;
using System.Security.Cryptography;

namespace FootballSquad.Core.Services
{
    public class TokenService : ITokenService
    {
        // length is 64
        public string GenerateToken()
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] chars = new char[64];
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[64];
                rng.GetBytes(bytes);
                for (int i = 0; i < 64; i++)
                {
                    chars[i] = validChars[bytes[i] % validChars.Length];
                }
            }
            return new string(chars);
        }
    }
}
