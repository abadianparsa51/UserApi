using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;

namespace UserApi.Data
{
    public class ApiDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<CardDetail> CardDetails { get; set; }

        public DbSet<CardPrefix> CardPrefixes { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<TransactionLog> TransactionLogs { get; set; } // درست کردن نام DbSet
        public DbSet<Fee> Fees { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ↔ CardDetail (One-to-Many)
            modelBuilder.Entity<CardDetail>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cards)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            // Bank ↔ CardDetail (One-to-Many)
            modelBuilder.Entity<CardDetail>()
                .HasOne(c => c.Bank)
                .WithMany(b => b.CardDetails)
                .HasForeignKey(c => c.BankId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade here

            // CardDetail ↔ TransactionLog (One-to-Many)
            modelBuilder.Entity<TransactionLog>()
                .HasOne(t => t.CardDetail)
                .WithMany()
                .HasForeignKey(t => t.CardDetailId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade here as well

            // Bank ↔ CardPrefix (One-to-Many)
            modelBuilder.Entity<CardPrefix>()
                .HasOne(p => p.Bank)
                .WithMany(b => b.CardPrefixes)
                .HasForeignKey(p => p.BankId);

            // ارتباط User ↔ Contact (One-to-Many)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            // اصلاح رابطه Contact ↔ CardDetail (One-to-Many)
            modelBuilder.Entity<CardDetail>()
                .HasOne(cd => cd.Contact)
                .WithMany(c => c.Cards)
                .HasForeignKey(cd => cd.ContactId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade here

            // تنظیم رابطه Contact ↔ TransactionLog (One-to-Many)
            modelBuilder.Entity<TransactionLog>()
                .HasOne(t => t.Contact)
                .WithMany(c => c.TransactionLogs)
                .HasForeignKey(t => t.ContactId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete
            // افزودن داده‌های پیش‌شماره کارت‌های بانکی
            // مشخص کردن داده‌های اولیه برای جدول Bank
            modelBuilder.Entity<Bank>().HasData(
                 new Bank { Id = 1, Name = "بانک ملی ایران", SwiftCode = "BKAB12" },
                 new Bank { Id = 2, Name = "بانک سپه", SwiftCode = "BKB123" },
                 new Bank { Id = 3, Name = "بانک اقتصاد نوین", SwiftCode = "BKEN34" },
                 new Bank { Id = 4, Name = "بانک سامان", SwiftCode = "BKSAM56" },
                 new Bank { Id = 5, Name = "بانک ملت", SwiftCode = "BKML78" },
                 new Bank { Id = 6, Name = "بانک صادرات ایران", SwiftCode = "BKEX90" },
                 new Bank { Id = 7, Name = "بانک کشاورزی", SwiftCode = "BKAG12" },
                 new Bank { Id = 8, Name = "بانک مسکن", SwiftCode = "BKHS34" },
                 new Bank { Id = 9, Name = "بانک پاسارگاد", SwiftCode = "BKPS56" },
                 new Bank { Id = 10, Name = "بانک پارسیان", SwiftCode = "BKPR78" }
             );
            modelBuilder.Entity<CardPrefix>()
                .HasIndex(c => c.Prefix)
                    .IsUnique();
            modelBuilder.Entity<CardPrefix>().HasData(
              new CardPrefix { Id = 1, Prefix = "603799", BankName = "بانک ملی ایران", BankId = 1 },
              new CardPrefix { Id = 2, Prefix = "589210", BankName = "بانک سپه", BankId = 2 },
              new CardPrefix { Id = 3, Prefix = "627412", BankName = "بانک اقتصاد نوین", BankId = 3 },
              new CardPrefix { Id = 4, Prefix = "621986", BankName = "بانک سامان", BankId = 4 },
              new CardPrefix { Id = 5, Prefix = "610433", BankName = "بانک ملت", BankId = 5 },
              new CardPrefix { Id = 6, Prefix = "603769", BankName = "بانک صادرات ایران", BankId = 6 },
              new CardPrefix { Id = 7, Prefix = "639217", BankName = "بانک کشاورزی", BankId = 7 },
              new CardPrefix { Id = 8, Prefix = "628023", BankName = "بانک مسکن", BankId = 8 },
              new CardPrefix { Id = 9, Prefix = "502229", BankName = "بانک پاسارگاد", BankId = 9 },
              new CardPrefix { Id = 10, Prefix = "622106", BankName = "بانک پارسیان", BankId = 10 },
              new CardPrefix { Id = 11, Prefix = "639347", BankName = "بانک پاسارگاد", BankId = 9 },
              new CardPrefix { Id = 12, Prefix = "627884", BankName = "بانک پارسیان", BankId = 10 },
              new CardPrefix { Id = 13, Prefix = "505785", BankName = "بانک ایران‌زمین", BankId = 6 },
              new CardPrefix { Id = 14, Prefix = "606373", BankName = "بانک قرض‌الحسنه مهر ایران", BankId = 8 }
          );

            // تنظیمات بیشتر برای TransactionLog


        }

    }
}
