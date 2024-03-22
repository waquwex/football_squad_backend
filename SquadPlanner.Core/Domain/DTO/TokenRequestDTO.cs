using System.ComponentModel.DataAnnotations;

namespace FootballSquad.Core.Domain.DTO
{
    public class TokenRequestDTO
    {
        [Required]
        [MinLength(620)]
        public string? JWTToken { get; set; }

        [Required]
        [StringLength(88)]
        public string? RefreshToken { get; set; }
    }
}