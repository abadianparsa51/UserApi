using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ContactDTO
{
    public int Id { get; set; }  // Add this Id property
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
}
