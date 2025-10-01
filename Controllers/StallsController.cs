using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasExpo.Controllers
{
    [AllowAnonymous]
    public class StallsController : Controller
    {
        public IActionResult Index(string search = null)
        {
            TempData["LegacyNotice"] = "We've refreshed our marketplace. Browse current availability in the events catalog instead.";
            return RedirectToAction("Index", "Events", new { search });
        }

        public IActionResult Details(int id)
        {
            TempData["LegacyNotice"] = "Individual stall pages have moved. Explore event schedules to check stall slots.";
            return RedirectToAction("Index", "Events");
        }
    }
}
