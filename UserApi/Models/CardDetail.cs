using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserApi.Models.DTOs;

namespace UserApi.Models
{
    public class CardDetail
    {
        [Key]
        public int CardDetailId { get; set; }

        [Column(TypeName = "nvarchar(16)")]
        [Required]
        public string CardNumber { get; set; } = "";

        [Column(TypeName = "nvarchar(5)")]
        [Required]
        public string ExpirationDate { get; set; } = "";
        [Required]
        public string UserId { get; set; }

    }
}
