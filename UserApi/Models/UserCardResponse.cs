namespace UserApi.Models
{
    internal class UserCardResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<CardDetail> Cards { get; set; }
    }
}