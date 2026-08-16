using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PasswordResetRequestViewModel
    {
        [Required(ErrorMessage = "Reset request Id is required.")] 
        public string Id { get; set; }

        [Required(ErrorMessage = "User Id is required.")] 
        public string UserId { get; set; } //It is int but taken as string as it comes in encode format
        
        [Required(ErrorMessage = "Username is Required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]        
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password not matched.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Token is Required.")] 
        public string? Token { get; set; }
        
        [Required(ErrorMessage = "Captcha is required.")]
        public string Captcha { get; set; }
    }
}