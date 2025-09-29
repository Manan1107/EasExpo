using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Stalls
{
    public class StallListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public decimal RentPerDay { get; set; }
        public string OwnerName { get; set; }
        public StallStatus Status { get; set; }
    }
}
