using UserApi.Models.DTOs;

namespace UserApi.Models
{
    public class LoginRequestResponse : AuthResult

    {
        public string UserId { get; internal set; }
    }
}
