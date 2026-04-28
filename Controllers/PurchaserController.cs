using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;

[Authorize(Roles = "P")]
[ApiController]
[Route("api/purchaser")]
public class PurchaserController : ControllerBase
{
    private readonly AppDbContext _context;
    public PurchaserController(AppDbContext context)
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
            message = "Purchaser Access",
            data = users
        });
    }
}