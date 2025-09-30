using System;
using System.Collections.Generic;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminBookingDetailViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalPaid { get; set; }
        public IReadOnlyList<AdminPaymentSummaryViewModel> Payments { get; set; } = new List<AdminPaymentSummaryViewModel>();
    }
}
