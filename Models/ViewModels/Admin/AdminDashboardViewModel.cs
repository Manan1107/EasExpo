namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveCustomers { get; set; }
        public int ActiveOwners { get; set; }
        public int TotalStalls { get; set; }
        public int AvailableStalls { get; set; }
        public int PendingOwnerApplications { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int FailedPayments { get; set; }
        public int TotalPayments { get; set; }
    }
}
