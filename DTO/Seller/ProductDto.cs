using System.ComponentModel.DataAnnotations;

public class ProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MinLength(2)]
    public string Name { get; set; }
    [Required(ErrorMessage = "Description is required")]
    [MinLength(5)]
    public string Description { get; set; }
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    [Required(ErrorMessage = "Stock is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }
    [Required(ErrorMessage = "Product image is required")]
    public IFormFile Image { get; set; }
}