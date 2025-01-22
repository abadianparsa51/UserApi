using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Models
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(20)]
        public string SwiftCode { get; set; } = "";

        // Navigation property
        public ICollection<CardDetail> CardDetails { get; set; }
        public ICollection<CardPrefix> CardPrefixes { get; set; }

    }
}
