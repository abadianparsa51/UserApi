namespace UserApi.DTOs
{
    public class CardDetailDTO
    {
        public int Id { get; set; }

        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV2 { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
    }
}
