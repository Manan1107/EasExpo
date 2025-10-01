using System;
using System.Collections.Generic;
using EasExpo.Models.ViewModels.Events;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerEventDetailsViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StallSize { get; set; }
        public decimal SlotPrice { get; set; }
        public string Description { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int BookedSlots { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public IReadOnlyList<EventSlotViewModel> Slots { get; set; }
    }
}
