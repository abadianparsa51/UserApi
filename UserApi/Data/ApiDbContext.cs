using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;

namespace UserApi.Data
{
    public class ApiDbContext : IdentityDbContext<ApplicationUser>
    {
        
        public DbSet<CardDetail> CardDetails { get; set; }
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CardDetail>()
                .HasOne(c => c.User)           // Each card belongs to one user
                .WithMany(u => u.Cards)        // Each user can have many cards
                .HasForeignKey(c => c.UserId)  // Foreign key property in Card
                .IsRequired();
        }
    }
}
