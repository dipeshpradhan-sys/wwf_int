using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class PinChangeViewModel
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

        [Required(ErrorMessage = "Pin is Required.")]
        [DataType(DataType.Password)]        
        public string NewPin { get; set; }

        [Required(ErrorMessage = "Confirm Pin is Required.")]
        [DataType(DataType.Password)]
        [Compare("NewPin", ErrorMessage = "Pin and confirm pin not matched.")]
        public string ConfirmPin { get; set; }

    }
}