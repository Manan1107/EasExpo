using System;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [Authorize(Roles = RoleNames.Customer + "," + RoleNames.StallOwner)]
    public class BookingsController : Controller
    {
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(EasExpoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int stallId)
        {
            var stall = await _context.Stalls.FindAsync(stallId);
            if (stall == null || stall.Status == StallStatus.Maintenance)
            {
                return NotFound();
            }

            var model = new BookingCreateViewModel
            {
                StallId = stall.Id,
                StallName = stall.Name,
                RentPerDay = stall.RentPerDay,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(2)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingCreateViewModel model)
        {
            var stall = await _context.Stalls.FindAsync(model.StallId);
            if (stall == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.StallName = stall.Name;
                model.RentPerDay = stall.RentPerDay;
                return View(model);
            }

            if (model.StartDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(model.StartDate), "Start date cannot be in the past.");
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date should be on or after the start date.");
            }

            var hasOverlap = await _context.Bookings
                .Where(b => b.StallId == model.StallId && b.Status != BookingStatus.Rejected)
                .AnyAsync(b => model.StartDate <= b.EndDate && model.EndDate >= b.StartDate);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty, "The stall is already booked for the selected dates.");
            }

            if (!ModelState.IsValid)
            {
                model.StallName = stall.Name;
                model.RentPerDay = stall.RentPerDay;
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var booking = new Booking
            {
                StallId = model.StallId,
                CustomerId = userId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending
            };

            _context.Bookings.Add(booking);
            stall.Status = StallStatus.Booked;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking request submitted. Please await owner approval.";
            return RedirectToAction(nameof(MyBookings));
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = await _context.Bookings
                .Include(b => b.Stall)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingListItemViewModel
                {
                    Id = b.Id,
                    StallName = b.Stall.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    Amount = CalculateAmount(b.StartDate, b.EndDate, b.Stall.RentPerDay)
                }).ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

            if (booking == null || booking.Status == BookingStatus.Rejected)
            {
                return NotFound();
            }

            if (booking.PaymentStatus == PaymentStatus.Completed)
            {
                TempData["Success"] = "This booking is already paid for.";
                return RedirectToAction(nameof(MyBookings));
            }

            var model = new PaymentCheckoutViewModel
            {
                BookingId = booking.Id,
                StallName = booking.Stall.Name,
                Amount = CalculateAmount(booking.StartDate, booking.EndDate, booking.Stall.RentPerDay)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentCheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == model.BookingId && b.CustomerId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.PaymentStatus == PaymentStatus.Completed)
            {
                TempData["Success"] = "Payment already processed.";
                return RedirectToAction(nameof(MyBookings));
            }

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = CalculateAmount(booking.StartDate, booking.EndDate, booking.Stall.RentPerDay),
                Provider = string.IsNullOrWhiteSpace(model.PaymentProvider) ? "Razorpay" : model.PaymentProvider,
                TransactionReference = string.IsNullOrWhiteSpace(model.TransactionReference)
                    ? $"SIM-{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : model.TransactionReference,
                Status = PaymentStatus.Completed,
                ProcessedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            booking.PaymentStatus = PaymentStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment processed successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> Feedback(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null || booking.Status != BookingStatus.Approved || booking.PaymentStatus != PaymentStatus.Completed || booking.EndDate > DateTime.UtcNow.Date)
            {
                TempData["Error"] = "Feedback can be submitted after the booking is completed.";
                return RedirectToAction(nameof(MyBookings));
            }

            var model = new FeedbackCreateViewModel
            {
                BookingId = booking.Id,
                StallName = booking.Stall.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(FeedbackCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == model.BookingId && b.CustomerId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingFeedback = await _context.Feedback.FirstOrDefaultAsync(f => f.BookingId == booking.Id);
            if (existingFeedback != null)
            {
                TempData["Error"] = "Feedback already submitted for this booking.";
                return RedirectToAction(nameof(MyBookings));
            }

            var feedback = new Feedback
            {
                BookingId = booking.Id,
                Rating = model.Rating,
                Comments = model.Comments
            };

            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback.";
            return RedirectToAction(nameof(MyBookings));
        }

        private static decimal CalculateAmount(DateTime start, DateTime end, decimal rentPerDay)
        {
            var days = (end.Date - start.Date).Days + 1;
            return Math.Max(days, 1) * rentPerDay;
        }
    }
}
