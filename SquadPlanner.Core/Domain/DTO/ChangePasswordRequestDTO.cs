using System.ComponentModel.DataAnnotations;

namespace FootballSquad.Core.Domain.DTO
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Current password can't be blank")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password length should be in range of {2} - {1}")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[._\\-\\*])[A-Za-z\\d._\\-\\*]{8,}$",
    ErrorMessage = "Password field should contain atleast one upper case one lower case and one of '*' '_' '-' '.' characters")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password can't be blank")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password length should be in range of {2} - {1}")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[._\\-\\*])[A-Za-z\\d._\\-\\*]{8,}$",
            ErrorMessage = "Password field should contain atleast one upper case one lower case and one of '*' '_' '-' '.' characters")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password field is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password and confirm password should match")]
        [DataType(DataType.Password)]
        public string? ConfirmNewPassword { get; set; }
    }
}