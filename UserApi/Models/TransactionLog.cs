using System.ComponentModel.DataAnnotations;
using UserApi.Models;

public class TransactionLog
{
    [Key]
    public int Id { get; set; }

    public string SourceCardNumber { get; set; }
    public string DestinationCardNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; }
    public string StatusMessage { get; set; } = "Default status message";
    public string TransactionId { get; set; }

    // Foreign key for CardDetail
    public int CardDetailId { get; set; }
    public CardDetail CardDetail { get; set; }

    // Foreign key for Contact
    public int ContactId { get; set; }
    public Contact Contact { get; set; }
}
