using FootballSquad.Core.Domain.DTO;


namespace FootballSquad.Core.ServiceContracts
{
    public interface IUserService
    {
        Task<bool> IsEmailExists(string email);
        Task<bool> IsUsernameExists(string username);
        Task Register(string email, string username, string password);
        Task<TokenResponseDTO> Login(string email, string password);
        Task Logout(Guid id);
        Task<TokenResponseDTO?> GetToken(string JWTToken, string refreshToken);
        Task<(Guid, string)?> GetIdAndUsernameFromEmail(string email);
        Task DeleteUser(Guid id);
        Task<(bool userExists, int? remainingSeconds)> ForgotPassword(string email);
        Task<bool> ChangePassword(string email, string currentPassword, string newPassword);
        Task<bool> ResetPassword(string email, string newPassword, string token);
    }
}