using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Admin
{
    public class PaymentReportItemViewModel
    {
        public int Id { get; set; }
        public string StallName { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Provider { get; set; }
        public string TransactionReference { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
