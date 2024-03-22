using FootballSquad.Core.Domain.DTO;
using FootballSquad.Core.ServiceContracts;
using FootballSquad.Core.Utils.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text;

namespace FootballSquad.Controllers.v1
{
    /// <summary>
    /// Account related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : CommonController
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Gets required services with DI
        /// </summary>
        /// <param name="userService"></param>
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] RegisterRequestDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // if email exists return email is exists problem
            var emailExists = await _userService.IsEmailExists(registerDTO.Email);
            // if username exists return username is exists problem
            var userNameExists = await _userService.IsUsernameExists(registerDTO.Username);

            if (emailExists && !userNameExists)
            {
                return Problem(title: "Email is already exists!");
            }
            else if (!emailExists && userNameExists)
            {
                return Problem(title: "Username is already exists!");
            }
            else if (emailExists && userNameExists)
            {
                return Problem(title: "Email and Username is already exists!");
            }

            await _userService.Register(registerDTO.Email, registerDTO.Username, registerDTO.Password);
            return Ok();
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginRequestDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authenticationResponse = await _userService.Login(loginDTO.Email, loginDTO.Password);
            if (authenticationResponse.Error?.Error != null)
            {
                if (authenticationResponse.Error?.Error == LoginError.USER_IS_LOCKOUT)
                {
                    return Problem(title: "Account is lockout",
                        detail: authenticationResponse.Error?.RemainingSeconds.ToString(),
                        statusCode: 429);
                }
                else if (authenticationResponse.Error?.Error == LoginError.PASSWORD_INCORRECT)
                {
                    return Problem(title: "Credentials are incorrect", statusCode: 417);
                }
                else if (authenticationResponse.Error?.Error == LoginError.USER_IS_NOT_EXIST)
                {
                    return Problem(title: "User is not exists!", statusCode: 404);
                }
            }

            return Ok(authenticationResponse);
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr);
            await _userService.Logout(userId);
        }

        [HttpPost]
        [Route("gettoken")]
        [AllowAnonymous]
        public async Task<ActionResult> GetToken([FromBody] TokenRequestDTO tokenDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authenticationResponse = 
                await _userService.GetToken(tokenDTO.JWTToken, tokenDTO.RefreshToken);
            
            if (authenticationResponse == null)
            {
                return Problem(title: "Failed to gather token", statusCode: 511);
            }

            return Ok(authenticationResponse);
        }

        /// <summary>
        /// Deletes account of current users
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete")]
        [Authorize]
        public async Task<ActionResult> DeleteAccount()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr);
            await _userService.DeleteUser(userId);
            return Ok();
        }

        [HttpPut]
        [Route("changePassword")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var success =  await _userService.ChangePassword(userEmail, changePasswordDTO.CurrentPassword,
                changePasswordDTO.NewPassword);
            
            if (success)
            {
                return Ok();
            }
            else
            {
                return Problem(title: "Invalid current password!", statusCode: 406);
            }
        }

        [HttpPost]
        [Route("forgotPassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequestDTO forgotPasswordRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var (userExists, lockoutRemainingSeconds) = 
                await _userService.ForgotPassword(forgotPasswordRequestDTO.Email);
            if (userExists)
            {
                if (lockoutRemainingSeconds != null)
                {
                    return Problem(title: "Reset password is lockout",
                        detail: lockoutRemainingSeconds.ToString(),
                        statusCode: 429);
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return Problem(title: "User is not exists!", statusCode: 404);
            }
        }

        [HttpPut]
        [Route("resetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDTO resetPasswordRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var success = await _userService.ResetPassword(resetPasswordRequestDTO.Email,
             resetPasswordRequestDTO.NewPassword, resetPasswordRequestDTO.Token);

            if (success)
            {
                return Ok();
            }
            else
            {
                return Problem(title: "Can't reset password. Invalid token or user is not exists!",
                    statusCode: 406);
            }
        }
    }
}