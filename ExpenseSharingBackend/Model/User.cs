using System.ComponentModel.DataAnnotations;

namespace ExpenseSharingBackend.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Role { get; set; } = "User";
        [Required]
        [MinLength(5)]
        public string Password { get; set; }

    }
}
