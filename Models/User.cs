using System.ComponentModel.DataAnnotations;

namespace ElabdStor.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!; // خزّن هاش وليس الباسورد النصي

        [Required, MaxLength(50)]
        public string Role { get; set; } = "Employee"; // "Admin" أو "Employee"
    }
}
