using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Data.ViewModels
{
    public class UserModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [AllowNull]
        public string FirstName { get; set; }

        [AllowNull]
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [AllowNull]
        public string Address { get; set; }
    }
}
