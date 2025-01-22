namespace UserApi.Models.DTOs
{
    public class TransferRequest
    {
        public string SourceCardNumber { get; set; } // شماره کارت مبدا
        public string DestinationCardNumber { get; set; } // شماره کارت مقصد
        public decimal Amount { get; set; } // مبلغ انتقال
        public required string UserId { get; set; } 
    }
}