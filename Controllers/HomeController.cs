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
                .Where(b => b.CustomerId == userId
                    && b.Status == BookingStatus.Approved
                    && b.PaymentStatus == PaymentStatus.Completed)
                .OrderByDescending(b => b.EndDate)
                .Select(b => new
                {
                    BookingId = b.Id,
                    HasFeedback = _context.Feedback.Any(f => f.BookingId == b.Id)
                })
                .Where(x => !x.HasFeedback)
                .FirstOrDefaultAsync();

            return RedirectToAction("Feedback", "Bookings", new { bookingId = booking.BookingId });
        }

        [Authorize(Roles = RoleNames.Customer + "," + RoleNames.StallOwner)]
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
