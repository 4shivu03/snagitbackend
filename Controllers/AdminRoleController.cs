using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;

[Authorize(Roles = "A")]
[ApiController]
[Route("api/admin/roles")]
public class AdminRoleController : ControllerBase
{
    private readonly AppDbContext _context;
    public AdminRoleController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.TblRoleMasters
            .OrderBy(r => r.Id)
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> AddRole([FromBody] TblRoleMaster model)
    {
        if (string.IsNullOrEmpty(model.RoleName) || string.IsNullOrEmpty(model.RoleCode))
            return BadRequest("Role name and code required");
        model.CreatedDate = DateTime.UtcNow;
        model.IsActive = true;
        _context.TblRoleMasters.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] TblRoleMaster model)
    {
        var role = await _context.TblRoleMasters.FindAsync(id);
        if (role == null)
            return NotFound();
        role.RoleName = model.RoleName;
        role.RoleCode = model.RoleCode;
        role.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(role);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> ToggleRoleStatus(int id)
    {
        var role = await _context.TblRoleMasters.FindAsync(id);
        if (role == null)
            return NotFound();
        role.IsActive = !role.IsActive;
        role.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new
        {
            role.Id,
            role.IsActive
        });
    }
}