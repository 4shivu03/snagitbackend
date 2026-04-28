using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using System.Security.Claims;

[Authorize(Roles = "S")]
[ApiController]
[Route("api/seller")]
public class SellerController : ControllerBase
{
    private readonly AppDbContext _context;
    public SellerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.TblUserLogins
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Mobile,
                u.Role,
                u.IsActive,
                u.CreatedDate
            })
            .ToListAsync();
        return Ok(new
        {
            message = "Seller Access",
            data = users
        });
    }

    [HttpPost("product")]
    public async Task<IActionResult> AddProduct(ProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var email = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _context.TblUserLogins
            .Include(x => x.SellerDetail)
            .FirstOrDefaultAsync(x => x.Email == email);
        if (user == null || user.SellerDetail == null)
            return Unauthorized();
        var file = dto.Image;
        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files allowed");
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("Max 2MB allowed");
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var path = Path.Combine("wwwroot/uploads", fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var product = new TblProduct
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = "/uploads/" + fileName,
            SellerId = user.Id,
            CreatedBy = user.Id
        };
        _context.TblProducts.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

   [HttpPut("product/{id}")]
public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
{
    var email = User.FindFirst(ClaimTypes.Name)?.Value;
    var user = await _context.TblUserLogins
        .Include(x => x.SellerDetail)
        .FirstOrDefaultAsync(x => x.Email == email);
    if (user == null || user.SellerDetail == null)
        return Unauthorized();
    var product = await _context.TblProducts.FindAsync(id);
    if (product == null)
        return NotFound();
    product.Name = dto.Name;
    product.Description = dto.Description;
    product.Price = dto.Price;
    product.Stock = dto.Stock;
    if (dto.Image != null)
    {
        var file = dto.Image;
        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files allowed");
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("Max 2MB allowed");
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var path = Path.Combine("wwwroot/uploads", fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        product.ImageUrl = "/uploads/" + fileName;
    }
    product.UpdatedBy = user.Id;
    product.UpdatedDate = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return Ok(product);
}

    [HttpGet("my-products")]
    public async Task<IActionResult> GetMyProducts()
    {
        var email = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _context.TblUserLogins
            .Include(x => x.SellerDetail)
            .FirstOrDefaultAsync(x => x.Email == email);
        var products = await _context.TblProducts
            .Where(x => x.SellerId == user.SellerDetail.Id)
            .ToListAsync();
        return Ok(products);
    }

}
