using System.ComponentModel.DataAnnotations;

namespace FootballSquad.Core.Domain.DTO
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}