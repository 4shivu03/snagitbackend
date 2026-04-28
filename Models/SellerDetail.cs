public class TblSellerDetailMaster
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public TblUserLogin User { get; set; }
    public string BusinessName { get; set; }
    public string GSTNumber { get; set; }
    public string PANNumber { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Pincode { get; set; }
    public string AccountHolderName { get; set; }
    public string AccountNumber { get; set; }
    public string IFSCCode { get; set; }
    public string BankName { get; set; }
    public string Status { get; set; } = "P";
    public string? RejectReason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}