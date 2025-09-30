using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasExpo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            EasExpoDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.Customer)]
        public async Task<IActionResult> Feedback()
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .Where(b => b.CustomerId == userId
                    && b.Status == BookingStatus.Approved
                    && b.PaymentStatus == PaymentStatus.Completed)
                .Where(b => !_context.Feedback.Any(f => f.BookingId == b.Id))
                .OrderByDescending(b => b.EndDate)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                TempData["Error"] = "You need at least one paid booking without feedback before leaving feedback.";
                return RedirectToAction("MyBookings", "Bookings");
            }

            return RedirectToAction("Feedback", "Bookings", new { bookingId = booking.Id });
        }

        [Authorize(Roles = RoleNames.Customer)]
        public IActionResult Support()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
