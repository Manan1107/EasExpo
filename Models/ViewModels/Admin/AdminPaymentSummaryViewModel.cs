using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminPaymentSummaryViewModel
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionReference { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
