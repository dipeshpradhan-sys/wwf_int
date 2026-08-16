using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PinResetRequestViewModel
    {
        [Required(ErrorMessage = "Reset request Id is required.")] 
        public string Id { get; set; }

        [Required(ErrorMessage = "User Id is required.")]
        public string UserId { get; set; } //It is int but taken as string as it comes in encode format

        [Required(ErrorMessage = "Username is Required.")] 
        public string? Username { get; set; }

        [Required(ErrorMessage = "Pin is Required.")]
        [MinLength(6, ErrorMessage = "Pin must be at least 6 characters Numbers.")]
        [DataType(DataType.Password)]
        public string Pin { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("Pin", ErrorMessage = "Pin and confirm pin not matched.")]
        public string ConfirmPin { get; set; }

        [Required(ErrorMessage = "Token is Required.")]
        public string? Token { get; set; }

        [Required(ErrorMessage = "Captcha is required.")]
        public string Captcha { get; set; }
    }
}