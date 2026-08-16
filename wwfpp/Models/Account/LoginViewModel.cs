using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class LoginViewModel
    {
        
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string RememberMe { get; set; }
        public string? Captcha { get; set; }
        public string? ShowCaptcha { get; set; }

    }
}