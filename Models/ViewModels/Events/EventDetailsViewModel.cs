using System;
using System.Collections.Generic;

namespace EasExpo.Models.ViewModels.Events
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StallSize { get; set; }
        public decimal SlotPrice { get; set; }
        public string Description { get; set; }
        public int AvailableSlots { get; set; }
        public int TotalSlots { get; set; }
        public IReadOnlyList<EventSlotViewModel> Slots { get; set; }
        public bool CanBook { get; set; }
    }
}
