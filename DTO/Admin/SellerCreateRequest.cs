using System.ComponentModel.DataAnnotations;

public class SellerCreateRequest
{
    [Required, MinLength(2)]
    public string FirstName { get; set; }
    [Required, MinLength(2)]
    public string LastName { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number")]
    public string Mobile { get; set; }
    [Required, MinLength(6)]
    public string Password { get; set; }
    [Required, MinLength(2)]
    public string BusinessName { get; set; }
    [Required]
    public string GSTNumber { get; set; }
    [Required]
    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN")]
    public string PANNumber { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string State { get; set; }
    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid pincode")]
    public string Pincode { get; set; }
    // 🔹 BANK
    [Required]
    public string AccountHolderName { get; set; }
    [Required]
    [MinLength(8)]
    public string AccountNumber { get; set; }
    [Required]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC")]
    public string IFSCCode { get; set; }
    [Required]
    public string BankName { get; set; }
}