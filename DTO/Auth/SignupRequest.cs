using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth
{
    public class SignupRequest
    {
        [Required, MinLength(2)]
        public string FirstName { get; set; } = null!;
        [Required, MinLength(2)]
        public string LastName { get; set; } = null!;
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number")]
        public string Mobile { get; set; } = null!;
        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
        [Required]
        public string? Role { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}