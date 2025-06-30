using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    public class LogIn
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
