using System.ComponentModel.DataAnnotations;
namespace UserApi.Models.DTOs
{
    public class CardDetailDto
    {
        [Required]
        public string CardNumber { get; set; } = "";
        [Required]
        public string ExpirationDate { get; set; } = "";
    }
}
