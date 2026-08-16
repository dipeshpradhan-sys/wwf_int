using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PasswordChangeViewModel
    {
        [Required(ErrorMessage = "Mode is required.")] // Edit or Add mode
        public string Mode { get; set; }

        [Required(ErrorMessage = "User Id is required.")] 
        public string UserId { get; set; } //It is int but taken as string as it comes in encode format
        
        [Required(ErrorMessage = "Username is Required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Old Password is Required.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]        
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and confirm password not matched.")]
        public string ConfirmPassword { get; set; }

    }
}