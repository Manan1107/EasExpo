using System;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerEventListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public decimal SlotPrice { get; set; }
    }
}
