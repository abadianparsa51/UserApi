namespace UserApi.Models
{
    public class Fee
    {
        public int Id { get; set; }
        public decimal FeeAmount { get; set; } // مقدار کارمزد
        public DateTime Date { get; set; } // تاریخ تراکنش
        public string SourceCardNumber { get; set; } // شماره کارت مبدا
        public string DestinationCardNumber { get; set; } // شماره کارت مقصد
        public string TransactionId { get; set; } // شناسه تراکنش
    }
}
