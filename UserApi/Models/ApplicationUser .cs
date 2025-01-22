using Microsoft.AspNetCore.Identity;

namespace UserApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<CardDetail> Cards { get; set; } // کارت‌های کاربر
                                                           // اضافه کردن ارتباط یک کاربر با چند Contact
        public ICollection<Contact> Contacts { get; set; }
    }
}
