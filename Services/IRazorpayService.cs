using System.Threading.Tasks;

namespace EasExpo.Services
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string currency, string receiptId);
        Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
    }
}
