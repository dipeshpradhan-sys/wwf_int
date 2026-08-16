using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PasswordForgotRequestViewModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        public string? Captcha { get; set; }
    }
}