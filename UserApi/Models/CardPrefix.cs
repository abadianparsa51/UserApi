namespace UserApi.Models
{
    public class CardPrefix
    {
        public int Id { get; set; } // کلید اصلی
        public string Prefix { get; set; } = string.Empty; // پیش‌شماره کارت
        public string BankName { get; set; } = string.Empty; // نام بانک
    }
}
