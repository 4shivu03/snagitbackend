using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using System.Security.Claims;

[Authorize(Roles = "A")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    public AdminController(AppDbContext context)
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
                u.Status,
                u.CreatedDate
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var user = await _context.TblUserLogins.FindAsync(id);
        if (user == null)
            return NotFound();
        user.IsActive = !user.IsActive; 
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new
        {
            user.Id,
            user.IsActive
        });
    }

    [HttpGet("sellers")]
    public async Task<IActionResult> GetSellers()
    {
        var data = await _context.TblSellerDetailMasters
            .Include(s => s.User)
            .Select(s => new
            {
                s.Id,
                s.BusinessName,
                s.GSTNumber,
                s.PANNumber,
                s.Address,
                s.City,
                s.State,
                s.Pincode,
                s.AccountHolderName,
                s.AccountNumber,
                s.IFSCCode,
                s.BankName,
                s.Status,
                s.CreatedDate,
                User = new
                {
                    s.User.Id,
                    s.User.FirstName,
                    s.User.LastName,
                    s.User.Email,
                    s.User.Mobile,
                    s.User.Status,      
                    s.User.IsActive,
                    s.User.CreatedDate
                }
            })
            .ToListAsync();
        return Ok(data); 
    }

    [HttpPut("seller/{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var email = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized("Invalid token");
        var adminUser = await _context.TblUserLogins
            .FirstOrDefaultAsync(u => u.Email == email);
        if (adminUser == null)
            return Unauthorized("User not found");
        var seller = await _context.TblSellerDetailMasters
         .FirstOrDefaultAsync(s => s.Id == id);
        if (seller == null)
            return NotFound();
        seller.Status = "A";
        seller.RejectReason = null;
        seller.UpdatedBy = adminUser.Id;
        seller.UpdatedDate = DateTime.UtcNow;
        var user = await _context.TblUserLogins
            .FirstOrDefaultAsync(u => u.Id == seller.UserId);
        if (user != null)
        {
            user.Status = "A";
            user.IsActive = true;
            user.UpdatedBy = adminUser.Id;
            user.UpdatedDate = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("seller/{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDto dto)
    {
        var email = User.FindFirst(ClaimTypes.Name)?.Value;
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        if (string.IsNullOrEmpty(email))
            return Unauthorized("Invalid token");
        var adminUser = await _context.TblUserLogins
            .FirstOrDefaultAsync(u => u.Email == email);
        if (adminUser == null)
            return Unauthorized("User not found");
        var seller = await _context.TblSellerDetailMasters
            .FirstOrDefaultAsync(s => s.Id == id);
        if (seller == null)
            return NotFound();
        seller.Status = "R";
        seller.RejectReason = dto.Reason;
        seller.UpdatedBy = adminUser.Id;
        seller.UpdatedDate = DateTime.UtcNow;
        var user = await _context.TblUserLogins
            .FirstOrDefaultAsync(u => u.Id == seller.UserId);
        if (user != null)
        {
            user.Status = "R";
            user.IsActive = false;
            user.UpdatedBy = adminUser.Id;
            user.UpdatedDate = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("seller/{id}")]
    public async Task<IActionResult> GetSeller(int id)
    {
        var s = await _context.TblSellerDetailMasters
            .Include(x => x.User)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.BusinessName,
                x.GSTNumber,
                x.PANNumber,
                x.Address,
                x.City,
                x.State,
                x.Pincode,
                x.AccountHolderName,
                x.AccountNumber,
                x.IFSCCode,
                x.BankName,
                x.Status,
                User = new
                {
                    x.User.FirstName,
                    x.User.LastName,
                    x.User.Email,
                    x.User.Mobile
                }
            })
            .FirstOrDefaultAsync();
        return Ok(s);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalUsers = await _context.TblUserLogins.CountAsync();
        var totalSellers = await _context.TblUserLogins
            .CountAsync(x => x.Role == "S");
        var totalPurchasers = await _context.TblUserLogins
            .CountAsync(x => x.Role == "P");
        var approvedUsers = await _context.TblUserLogins
            .CountAsync(x => x.Status == "A");
        var pendingUsers = await _context.TblUserLogins
            .CountAsync(x => x.Status == "P");
        var rejectedUsers = await _context.TblUserLogins
            .CountAsync(x => x.Status == "R");
        var activeUsers = await _context.TblUserLogins
            .CountAsync(x => x.IsActive == true);
        var deactivatedUsers = await _context.TblUserLogins
            .CountAsync(x => x.IsActive == false);
        var last7DaysActive = await _context.TblUserLogins
            .CountAsync(x => x.LastLoginDate != null &&
                             x.LastLoginDate >= DateTime.UtcNow.AddDays(-7));

        return Ok(new
        {
            totalUsers,
            totalSellers,
            totalPurchasers,
            approvedUsers,
            pendingUsers,
            rejectedUsers,
            activeUsers,
            deactivatedUsers,
            last7DaysActive
        });
    }

    [HttpGet("usergrowth")]
    public async Task<IActionResult> GetUserGrowth()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();
        var data = new List<object>();
        foreach (var day in last7Days)
        {
            var count = await _context.TblUserLogins
                .CountAsync(x => x.CreatedDate.Date == day);
            data.Add(new
            {
                date = day.ToString("dd MMM"),
                users = count
            });
        }
        return Ok(data);
    }

    [HttpGet("rolestats")]
    public async Task<IActionResult> GetRoleStats()
    {
        var data = await _context.TblUserLogins
            .GroupBy(u => u.Role)
            .Select(g => new
            {
                role = g.Key,
                count = g.Count()
            })
            .ToListAsync();
        return Ok(data);
    }
}