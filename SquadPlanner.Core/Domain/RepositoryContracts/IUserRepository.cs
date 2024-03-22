namespace FootballSquad.Core.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<bool> IsEmailExists(string email);
        Task<bool> IsUsernameExists(string username);
        Task<Guid> Create(string email, string username, string password);
        Task SaveRefreshTokenAndExpiration(Guid userId, string refreshToken, DateTime refreshTokenExpirationTime);
        Task <(Guid, string)?> GetIdAndUsernameFromEmail(string email);
        Task<bool> IsRefreshTokenValid(string email, string refreshTokenToCompare);
        Task DeleteRefreshToken(Guid id);
        Task DeleteUser(Guid id);
        Task<bool> CheckPassword(string email, string password);
        Task ChangePassword(string email, string newPassword);
        Task<(bool Lockout, int? RemainingTime)> IsLoginLockout(string email); 
        Task ResetLoginLockout(string email);
        Task InvalidLogin(string email);
        Task<(bool lockout, int? remainingTime)> IsForgotPasswordLockout(string email);
        Task ForgotPassword(string email, string token);
        Task<bool> ResetPassword(string email, string newPassword, string token);
    }
}