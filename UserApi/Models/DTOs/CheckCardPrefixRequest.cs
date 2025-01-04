using System.ComponentModel.DataAnnotations;

namespace UserApi.Models.DTOs
{
    public class CheckCardPrefixRequest
    {
        [Required]
        [MinLength(6)]
        public string CardNumber { get; set; } = string.Empty;
    }
}
