using System.ComponentModel.DataAnnotations;

namespace FootballSquad.Core.Domain.DTO
{
    public class LoginRequestDTO
    {

        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email should be in a proper email addres format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password can't be blank")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password length should be in range of {2} - {1}")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[._\\-\\*])[A-Za-z\\d._\\-\\*]{8,}$",
            ErrorMessage = "Password field should contain atleast one upper case one lower case and one of '*' '_' '-' '.' characters")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}