using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApi.Models
{
    public class CardDetail
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(16)")]
        public string CardNumber { get; set; } = "";

        [MaxLength(10)]
        public string ExpirationDate { get; set; } = "";

        [MaxLength(4)]
        public string CVV2 { get; set; } = "";

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [MaxLength]
        public decimal Balance { get; set; } // موجودی حساب

        // Foreign key for Bank
        public int BankId { get; set; }
        public Bank Bank { get; set; }

        // ارتباط یک به چند با تراکنش‌ها
        public ICollection<TransactionLog> TransactionLogs { get; set; }  // یک کارت می‌تواند چند تراکنش داشته باشد

        [NotMapped]
        public string BankName { get; set; }

        // Foreign key برای Contact
        public int ContactId { get; set; }

        // ویژگی ناوبری برای Contact
        public Contact Contact { get; set; }
    }
}
