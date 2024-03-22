using Dapper;
using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;


namespace FootballSquad.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:Footballers"];
        }

        public async Task<bool> IsEmailExists(string email)
        {
            var sql = "select count(email) from Users where email = @Email";
            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var count = await connection.QuerySingleAsync<int>(sql, sqlParams);

                return count > 0;
            }
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            var sql = "select count(username) from Users where username = @Username";
            var sqlParams = new
            {
                Username = username
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var count = await connection.QuerySingleAsync<int>(sql, sqlParams);
                return count > 0;
            }
        }

        public async Task<Guid> Create(string email, string username, string password)
        {          
            // Generate password hash and salt to save DB
            var salt = RandomNumberGenerator.GetBytes(64);
            var saltStr = Convert.ToHexString(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                350000,
                HashAlgorithmName.SHA512,
                64
                );
            var hashStr = Convert.ToHexString(hash);

            var sqlParams = new
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                HashedPassword = hashStr,
                Salt = saltStr,
            };

            var sql = "insert into Users(id, username, email, hashedPassword, salt)" +
                " OUTPUT Inserted.id values(@Id, @Username, @Email, @HashedPassword, @Salt)";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Guid newlyInsertedUserId;
                newlyInsertedUserId = await connection.QuerySingleAsync<Guid>(sql, sqlParams);
                return newlyInsertedUserId;
            }
        }

        public async Task SaveRefreshTokenAndExpiration(Guid userId, string refreshToken, DateTime refreshTokenExpirationTime)
        {
            var sqlParams = new
            {
                UserId = userId,
                RefreshToken = refreshToken,
                RefreshTokenExpirationTime = refreshTokenExpirationTime
            };

            var sql = "update Users set refreshToken = @RefreshToken, refreshTokenExpirationTime=@RefreshTokenExpirationTime " +
                "where id = @UserId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var affected = await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task<(Guid, string)?> GetIdAndUsernameFromEmail(string email)
        {
            var sqlParams = new
            {
                Email = email
            };
            var sql = "select id, username from Users where email = @Email";

            (Guid, string)? result;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                result = await connection.QueryFirstOrDefaultAsync<(Guid, string)?>(sql, sqlParams);
                return result;
            }
        }

        public async Task<bool> IsRefreshTokenValid(string email, string refreshTokenToCompare)
        {
            var sqlParams = new
            {
                Email = email
            };

            var sql = "select refreshToken, refreshTokenExpirationTime from Users " +
                "where email = @Email";

            (string refreshToken, DateTime refreshTokenExpirationTime)? result;

            using (var connection = new SqlConnection(_connectionString))
            {
                result = await connection.QueryFirstOrDefaultAsync<(string, DateTime)?>(sql, sqlParams);
            }


            if (result == null)
            {
                return false;
            }

            if ((refreshTokenToCompare == result?.refreshToken) && (result?.refreshTokenExpirationTime > DateTime.UtcNow))
            {
                return true;
            }

            return false;
        }

        public async Task DeleteRefreshToken(Guid id)
        {
            var sqlParams = new
            {
                Id = id
            };

            var sql = "update Users set refreshToken = NULL, refreshTokenExpirationTime = NULL " +
                "where id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task DeleteUser(Guid id)
        {
            var sql = "delete Users where id = @UserId";
            var sqlParams = new
            {
                UserId = id
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task<bool> CheckPassword(string email, string password)
        {
            var sql = "select hashedPassword, salt from Users where email = @Email";
            var sqlParams = new
            {
                Email = email
            };

            (string, string)? hashedPasswordAndSalt;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                hashedPasswordAndSalt = await connection.QueryFirstOrDefaultAsync<(string, string)?>(sql, sqlParams);
            }

            if (hashedPasswordAndSalt == null)
            {
                await Console.Out.WriteLineAsync("Hashed password and salt is null. Email is not exists!");
                return false;
            }

            var (hash, salt) = hashedPasswordAndSalt.Value;

            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), Convert.FromHexString(salt),
                350000, HashAlgorithmName.SHA512, 64);
            var success = CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
            if (!success)
            {
                await Console.Out.WriteLineAsync("Password doesn't match!");
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task ChangePassword(string email, string newPassword)
        {
            // Generate password hash and salt to save DB
            var salt = RandomNumberGenerator.GetBytes(64);
            var saltStr = Convert.ToHexString(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(newPassword),
                salt,
                350000,
                HashAlgorithmName.SHA512,
                64
                );
            var hashStr = Convert.ToHexString(hash);
            var sql = "update Users set hashedPassword = @HashedPassword, salt = @Salt " +
                "where email = @Email";
            var sqlParams = new
            {
                Email = email,
                HashedPassword = hashStr,
                Salt = saltStr
            };


            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task<(bool, int?)> IsLoginLockout(string email)
        {
            var sql = "select email as Email, failedLoginCount as FailedLoginCount, " +
                "DATEDIFF(second, failedLoginTime, getutcdate()) as PassedSecondsFromLastFail " +
                "from Users " +
                "where email = @Email";
            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.QueryFirstOrDefaultAsync<ApplicationUser>
                    (sql, sqlParams);
                if ((user.FailedLoginCount >= 5) && (user.PassedSecondsFromLastFail < 600))
                {
                    return (true, 600 - user.PassedSecondsFromLastFail);
                }
                else
                {
                    return (false, null);
                }
            }
        }

        public async Task ResetLoginLockout(string email)
        {
            var sql = "update Users set failedLoginCount = 0 " +
                "where email = @Email";
            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var affected = await connection.ExecuteAsync(sql, sqlParams);
                if (affected == 0)
                {
                    throw new Exception("Tried to ResetLoginLockout with not-existed email!");
                }
            }
        }

        public async Task InvalidLogin(string email)
        {
            var sql = "update Users SET failedLoginCount = CASE WHEN " +
                "DATEDIFF(minute, failedLoginTime, getutcdate()) >= 1 THEN 1 " +
                "WHEN failedLoginCount IS NULL THEN 1 ELSE failedLoginCount + 1 END, " +
                "failedLoginTime = getutcdate() WHERE email = @Email";

            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task ForgotPassword(string email, string token)
        {
            var sql = "update Users SET forgotPasswordToken = @Token, " +
                "forgotPasswordRequestTime = getutcdate() where email = @Email";

            var sqlParams = new
            {
                Email = email,
                Token = token
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var affected = await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        public async Task<(bool lockout, int? remainingTime)> IsForgotPasswordLockout(string email)
        {
            var sql = "select DATEDIFF(SECOND, forgotPasswordRequestTime, " +
                "getutcdate()) as ForgotPasswordRequestPassedTime from Users " +
                "where email = @Email";

            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var forgotPasswordRequestPassedTime = await connection.QueryFirstAsync<int?>(sql, sqlParams);
                if (forgotPasswordRequestPassedTime == null)
                {
                    return (false, null);
                }
                else
                {
                    if (forgotPasswordRequestPassedTime < 600)
                    {
                        return (true, 600 - forgotPasswordRequestPassedTime);
                    }
                    else
                    {
                        return (false, null);
                    }
                }
            }
        }

        public async Task<bool> ResetPassword(string email, string newPassword, string token)
        {
            var sql = "select email as Email, forgotPasswordToken as ForgotPasswordToken from Users " +
                "where email = @Email";
            var sqlParams = new
            {
                Email = email
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.QueryFirstOrDefaultAsync<ApplicationUser>(sql, sqlParams);

                if (user == null)
                {
                    return false;
                }

                if (user.ForgotPasswordToken == token)
                {
                    var resetForgotPasswordColumnsSql = "update Users " +
                        "set forgotPasswordRequestTime = null, forgotPasswordToken = null " +
                        "where email = @Email";
                    var resetForgotPasswordColumnsSqlParams = new
                    {
                        Email = user.Email
                    };
                    await connection.ExecuteAsync(resetForgotPasswordColumnsSql, resetForgotPasswordColumnsSqlParams);
                    
                    await ChangePassword(email, newPassword);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}