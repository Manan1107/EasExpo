namespace EasExpo.Services
{
    public class RazorpayOrderResult
    {
        public string OrderId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Receipt { get; set; }
    }
}
