using Microsoft.AspNetCore.Identity;

namespace UserApi.Models
{
    public class ApplicationUser :IdentityUser
    {
        public required ICollection<CardDetail> CardDetails { get; set; }
    }
}
