using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User ID / Employee Code")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
