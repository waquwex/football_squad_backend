using FootballSquad.Core.Domain.DTO;
using FootballSquad.Core.Domain.Entities;
using System.Security.Claims;

namespace FootballSquad.Core.ServiceContracts
{
    public interface IJWTService
    {
        TokenResponseDTO CreateJWTToken(Guid userId, string username, string email);
        ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string token);
    }
}