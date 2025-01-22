using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserApi.Models;

public class Contact
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(11)]
    public string Phone { get; set; } = "";

    [MaxLength(100)]
    public string Mail { get; set; } = "";
    [Column(TypeName = "nvarchar(16)")]
    public string DestinationCardNumber { get; set; } = "";

    [Required]
    public string UserId { get; set; } // کلید خارجی به ApplicationUser
    public ApplicationUser User { get; set; } // Navigation Property

    public ICollection<CardDetail> Cards { get; set; } // ارتباط یک Contact با چند Card

    // Navigation Property برای TransactionLogs
    public ICollection<TransactionLog> TransactionLogs { get; set; } // ارتباط یک Contact با چند تراکنش
}
