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
        [Column(TypeName = "nvarchar(5)")]
        public string ExpirationDate { get; set; } = "";
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
