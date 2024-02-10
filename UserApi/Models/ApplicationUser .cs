using Microsoft.AspNetCore.Identity;

namespace UserApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<CardDetail> Cards { get; set; }
    }
}
 