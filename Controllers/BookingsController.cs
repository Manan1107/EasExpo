using System;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.Options;
using EasExpo.Models.ViewModels.Bookings;
using EasExpo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasExpo.Controllers
{
    [Authorize(Roles = RoleNames.Customer)]
    public class BookingsController : Controller
    {
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRazorpayService _razorpayService;
        private readonly RazorpayOptions _razorpayOptions;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            EasExpoDbContext context,
            UserManager<ApplicationUser> userManager,
            IRazorpayService razorpayService,
            IOptions<RazorpayOptions> razorpayOptions,
            ILogger<BookingsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _razorpayService = razorpayService;
            _razorpayOptions = razorpayOptions?.Value ?? new RazorpayOptions();
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int stallId)
        {
            var stall = await _context.Stalls.FindAsync(stallId);
            if (stall == null || stall.Status == StallStatus.Maintenance || stall.Status == StallStatus.Booked)
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
                .Where(b => b.StallId == model.StallId && b.Status != BookingStatus.Rejected && b.Status != BookingStatus.Cancelled)
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
                Status = BookingStatus.Approved,
                PaymentStatus = PaymentStatus.Pending
            };

            _context.Bookings.Add(booking);
            stall.Status = StallStatus.Maintenance;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Pay), new { id = booking.Id });
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
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Rejected || booking.Status == BookingStatus.Cancelled)
            {
                TempData["Error"] = "This booking isn't eligible for payment.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (string.IsNullOrWhiteSpace(_razorpayOptions.KeyId) || string.IsNullOrWhiteSpace(_razorpayOptions.KeySecret))
            {
                _logger.LogError("Razorpay keys are not configured. Payment initiation blocked for booking {BookingId}.", booking.Id);
                TempData["Error"] = "Payment gateway is not configured. Please contact support.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.PaymentStatus == PaymentStatus.Completed)
            {
                TempData["Success"] = "This booking is already paid for.";
                return RedirectToAction(nameof(MyBookings));
            }

            var amount = CalculateAmount(booking.StartDate, booking.EndDate, booking.Stall.RentPerDay);

            try
            {
                var receipt = $"BK-{booking.Id}-{DateTime.UtcNow.Ticks}";
                var order = await _razorpayService.CreateOrderAsync(amount, "INR", receipt);

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.Status == PaymentStatus.Pending);

                if (payment == null)
                {
                    payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = amount,
                        Provider = "Razorpay",
                        TransactionReference = order.OrderId,
                        Status = PaymentStatus.Pending,
                        ProcessedAt = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);
                }
                else
                {
                    payment.Amount = amount;
                    payment.TransactionReference = order.OrderId;
                    payment.Status = PaymentStatus.Pending;
                    payment.ProcessedAt = DateTime.UtcNow;
                }

                booking.PaymentStatus = PaymentStatus.Pending;
                await _context.SaveChangesAsync();

                var model = new PaymentCheckoutViewModel
                {
                    BookingId = booking.Id,
                    StallId = booking.StallId,
                    StallName = booking.Stall.Name,
                    Amount = amount,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    Currency = order.Currency,
                    RazorpayOrderId = order.OrderId,
                    RazorpayKey = _razorpayOptions.KeyId,
                    CustomerName = booking.Customer?.FullName ?? booking.Customer?.UserName,
                    CustomerEmail = booking.Customer?.Email,
                    CustomerContact = booking.Customer?.PhoneNumber,
                    Notes = $"Booking #{booking.Id}"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Razorpay order for booking {BookingId}", booking.Id);
                TempData["Error"] = "We couldn't start the payment. Please try again in a moment.";
                return RedirectToAction(nameof(MyBookings));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePayment(PaymentConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid payment confirmation received.";
                return RedirectToAction(nameof(MyBookings));
            }

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

            var amount = CalculateAmount(booking.StartDate, booking.EndDate, booking.Stall.RentPerDay);
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.TransactionReference == model.RazorpayOrderId && p.Status == PaymentStatus.Pending);

            if (payment == null)
            {
                payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = amount,
                    Provider = "Razorpay",
                    TransactionReference = model.RazorpayOrderId,
                    Status = PaymentStatus.Pending,
                    ProcessedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
            }
            else
            {
                payment.Amount = amount;
            }

            bool isValid;
            try
            {
                isValid = await _razorpayService.VerifyPaymentAsync(model.RazorpayOrderId, model.RazorpayPaymentId, model.RazorpaySignature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Razorpay signature for booking {BookingId}", booking.Id);
                payment.Status = PaymentStatus.Failed;
                payment.ProcessedAt = DateTime.UtcNow;
                booking.PaymentStatus = PaymentStatus.Failed;
                booking.Status = BookingStatus.Cancelled;
                if (booking.Stall != null)
                {
                    booking.Stall.Status = StallStatus.Available;
                }
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Error"] = "We couldn't verify the payment signature. Please contact support if you were charged.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (!isValid)
            {
                payment.Status = PaymentStatus.Failed;
                payment.ProcessedAt = DateTime.UtcNow;
                booking.PaymentStatus = PaymentStatus.Failed;
                booking.Status = BookingStatus.Cancelled;
                if (booking.Stall != null)
                {
                    booking.Stall.Status = StallStatus.Available;
                }
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Error"] = "Payment verification failed. No charges were captured.";
                return RedirectToAction(nameof(MyBookings));
            }

            payment.Status = PaymentStatus.Completed;
            payment.TransactionReference = model.RazorpayPaymentId;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.Provider = $"Razorpay (Order: {model.RazorpayOrderId})";
            booking.PaymentStatus = PaymentStatus.Completed;
            booking.Status = BookingStatus.Approved;
            if (booking.Stall != null)
            {
                booking.Stall.Status = StallStatus.Booked;
            }
            booking.UpdatedAt = DateTime.UtcNow;

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
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == model.BookingId && b.CustomerId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.StallName = booking.Stall?.Name;
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
                Rating = null,
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
