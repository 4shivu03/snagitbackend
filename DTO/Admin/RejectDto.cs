using System.ComponentModel.DataAnnotations;
public class RejectDto
{
    [Required(ErrorMessage = "Reason is required")]
    [MinLength(5, ErrorMessage = "Reason must be at least 5 characters")]
    [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
    public string Reason { get; set; }
}
