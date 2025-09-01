
using System.ComponentModel.DataAnnotations;

namespace OnlineBookstore.ViewModels
{
    // Represents the registration form data for a new user
    public class RegisterVm
    {
        [Required, Display(Name = "Username")]
        [RegularExpression(@"^[A-Za-z0-9_.-]{3,30}$",
            ErrorMessage = "Use 3–30 letters, numbers or . _ -")]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        [DataType(DataType.Password), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
