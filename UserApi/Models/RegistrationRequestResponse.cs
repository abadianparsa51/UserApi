using UserApi.Models.DTOs;

namespace UserApi.Models
{
    public class RegistrationRequestResponse : AuthResult
    {
        public string UserId { get; internal set; }
    }
}
