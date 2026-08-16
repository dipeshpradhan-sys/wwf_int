using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PinForgotRequestViewModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string? Captcha { get; set; }
    }
}