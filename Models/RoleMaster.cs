public class TblRoleMaster
{
    public int Id { get; set; }
    public string RoleName { get; set; }
    public string RoleCode { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}