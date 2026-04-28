using System.ComponentModel.DataAnnotations;

public class ResetDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid otp")]
    public string Otp { get; set; }
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }
}