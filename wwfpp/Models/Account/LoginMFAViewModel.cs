using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class LoginMFAViewModel
    {

        [Required(ErrorMessage = "User Id is required.")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Pin is required.")]
        public string? Pin { get; set; }

    }
}