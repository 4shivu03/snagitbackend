using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<TblUserLogin> TblUserLogins { get; set; }
        public DbSet<TblSellerDetailMaster> TblSellerDetailMasters { get; set; }
        public DbSet<TblRoleMaster> TblRoleMasters { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TblOtp> TblOtps { get; set; }
        public DbSet<TblProduct> TblProducts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TblUserLogin>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<TblUserLogin>()
                .HasIndex(u => u.Mobile)
                .IsUnique();
            modelBuilder.Entity<TblSellerDetailMaster>()
                .HasOne(s => s.User)
                .WithOne(u => u.SellerDetail)
                .HasForeignKey<TblSellerDetailMaster>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}