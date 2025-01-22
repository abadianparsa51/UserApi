namespace UserApi.Models
{
    public class CardPrefix
    {
        public int Id { get; set; } // کلید اصلی
        public string Prefix { get; set; } = string.Empty; // پیش‌شماره کارت
        public string BankName { get; set; } = string.Empty; // نام بانک
        public int BankId { get; set; } // شناسه بانک
        public Bank Bank { get; set; }        // ارتباط با بانک
    }
}
