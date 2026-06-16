using System.ComponentModel.DataAnnotations;

namespace MYGROCER.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        // Automatically set to "Customer" when registering
        public string Role { get; set; } = "Customer"; 
    }
}