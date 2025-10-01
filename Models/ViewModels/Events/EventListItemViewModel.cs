using System;

namespace EasExpo.Models.ViewModels.Events
{
    public class EventListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StallSize { get; set; }
        public decimal SlotPrice { get; set; }
        public int AvailableSlots { get; set; }
        public int TotalSlots { get; set; }
    }
}
