using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Events
{
    public class EventSlotViewModel
    {
        public int StallId { get; set; }
        public int SlotNumber { get; set; }
        public StallStatus Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal RentPerDay { get; set; }
        public bool HasBookings { get; set; }
    }
}
