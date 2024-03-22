using FootballSquad.Core.Utils.Error;

namespace FootballSquad.Core.Domain.DTO
{
    public class TokenResponseDTO
    {
        public (LoginError Error, int RemainingSeconds)? Error { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
    }
}