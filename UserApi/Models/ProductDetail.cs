using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApi.Models
{
    public class ProductDetail
    {
        [Key]
        public int ProductId { get; set; }

        [Column(TypeName = "nvarchar(128)")]
        public string ProductName { get; set; } = "";

        [Column(TypeName = "nvarchar(128)")] // Corrected the decimal type definition
        public decimal ProductPrice { get; set; }
    }
}
