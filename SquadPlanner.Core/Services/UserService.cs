using FootballSquad.Core.Domain.DTO;
using FootballSquad.Core.Domain.RepositoryContracts;
using FootballSquad.Core.ServiceContracts;
using System.Security.Claims;
using FootballSquad.Core.Utils.Error;

namespace FootballSquad.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJWTService _JWTService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IJWTService JWTService, ITokenService tokenService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _JWTService = JWTService;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<bool> IsEmailExists(string email)
        {
            return await _userRepository.IsEmailExists(email);
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            return await _userRepository.IsUsernameExists(username);
        }

        public async Task Register(string email, string username, string password)
        {
            // Create user in DB
            var userId = await _userRepository.Create(email, username, password);
        }

        public async Task<TokenResponseDTO> Login(string email, string password)
        {
            var idAndUsernameTuple = await _userRepository.GetIdAndUsernameFromEmail(email);
            if (idAndUsernameTuple == null)
            {
                return new TokenResponseDTO
                {
                    Error = (LoginError.USER_IS_NOT_EXIST, 0)
                };
            }

            var (lockout, remainingTime) = await _userRepository.IsLoginLockout(email);

            if (lockout)
            {
                return new TokenResponseDTO
                {
                    Error = (LoginError.USER_IS_LOCKOUT, remainingTime.Value)
                };
            }
            
            var passwordCheck = await _userRepository.CheckPassword(email, password);
            if (!passwordCheck)
            {
                // Login fails
                await _userRepository.InvalidLogin(email);
                return new TokenResponseDTO
                {
                    Error = (LoginError.PASSWORD_INCORRECT, 0)
                };
            }

            // Login success
            await _userRepository.ResetLoginLockout(email);

            var (userId, username) = idAndUsernameTuple.Value;
            var authenticationResponse = _JWTService.CreateJWTToken(userId, username, email);
            await _userRepository.SaveRefreshTokenAndExpiration(userId, authenticationResponse.RefreshToken,
                authenticationResponse.RefreshTokenExpiration.Value
                );

            return authenticationResponse;
        }

        // Gets new token if refreshToken is valid
        public async Task<TokenResponseDTO?> GetToken(string JWTToken, string refreshToken)
        {
            var principal = _JWTService.GetClaimsPrincipalFromJwtToken(JWTToken);
            if (principal == null)
            {
                return null;
            }
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return null;
            }
            var userIdAndUserNameTuple = await _userRepository.GetIdAndUsernameFromEmail(email);
            if (userIdAndUserNameTuple == null)
            {
                return null;
            }

            var refreshTokenValid = await _userRepository.IsRefreshTokenValid(email, refreshToken);
            if (!refreshTokenValid)
            {
                return null;
            }

            var (userId, username) = userIdAndUserNameTuple.Value;

            var authenticationResponse = _JWTService.CreateJWTToken(userId, username, email);
            await _userRepository.SaveRefreshTokenAndExpiration(userId, authenticationResponse.RefreshToken,
                authenticationResponse.RefreshTokenExpiration.Value
                );
            return authenticationResponse;
        }

        public async Task Logout(Guid id)
        {
            await _userRepository.DeleteRefreshToken(id);
        }
    
        public async Task<(Guid, string)?> GetIdAndUsernameFromEmail(string email)
        {
            return await _userRepository.GetIdAndUsernameFromEmail(email);
        }

        public async Task DeleteUser(Guid id)
        {
            await _userRepository.DeleteUser(id);
        }

        public async Task<bool> ChangePassword(string email, string currentPassword,
            string newPassword)
        {
            var passwordCorrect = await _userRepository.CheckPassword(email, currentPassword);
            if (!passwordCorrect)
            {
                return false;
            }

            await _userRepository.ChangePassword(email, newPassword);
            return true;
        }

        // Return lockout remaining seconds if it forgot password lockedout
        public async Task<(bool userExists, int? remainingSeconds)> ForgotPassword(string email)
        {
            var idAndUsernameTuple = await _userRepository.GetIdAndUsernameFromEmail(email);
            if (idAndUsernameTuple == null)
            {
                return (false, null);
            }

            var (lockout, remainingSeconds) = await _userRepository.IsForgotPasswordLockout(email);
            if (lockout)
            {
                return (true, remainingSeconds);
            }
            else
            {
                var token = _tokenService.GenerateToken();
                _emailService.SendForgotPasswordEmail(email, token);
                await _userRepository.ForgotPassword(email, token);
            }

            return (true, null);
        }

        public async Task<bool> ResetPassword(string email, string newPassword, string token)
        {
            var (lockout, remainingSeconds) = await _userRepository.IsForgotPasswordLockout(email);
            if (lockout)
            {
                return await _userRepository.ResetPassword(email, newPassword, token);
            }
            else
            {
                return false;
            }
        }
    }
}