using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class LoginStepSetViewModel
    {
        [Required(ErrorMessage = "Mode is required.")] // Edit or Add mode
        public string Mode { get; set; }

        [Required(ErrorMessage = "User Id is required.")] 
        public string UserId { get; set; } //It is int but taken as string as it comes in encode format
        
        [Required(ErrorMessage = "Username is Required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Login Step Required.")]
        public int SignInType { get; set; }

        [DataType(DataType.Password)]
        public string Pin { get; set; }

    }
}