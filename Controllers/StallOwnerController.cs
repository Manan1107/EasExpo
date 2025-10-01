using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.StallOwner;
using EasExpo.Models.ViewModels.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [Authorize(Roles = RoleNames.StallOwner)]
    public class StallOwnerController : Controller
    {
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StallOwnerController(EasExpoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var model = await BuildDashboardModelAsync(userId);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            var model = new OwnerEventFormViewModel
            {
                StartDate = DateTime.UtcNow.Date.AddDays(7),
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                TotalSlots = 10,
                SlotPrice = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(OwnerEventFormViewModel model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date should be on or after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var eventEntity = new Event
            {
                Name = model.Name,
                Location = model.Location,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StallSize = model.StallSize,
                SlotPrice = model.SlotPrice,
                TotalSlots = model.TotalSlots,
                Description = model.Description,
                OwnerId = userId
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            for (var slot = 1; slot <= model.TotalSlots; slot++)
            {
                var stall = new Stall
                {
                    EventId = eventEntity.Id,
                    SlotNumber = slot,
                    Name = $"{model.Name} · Slot {slot}",
                    Location = model.Location,
                    Size = model.StallSize,
                    RentPerDay = model.SlotPrice,
                    Description = model.Description,
                    Status = StallStatus.Available,
                    OwnerId = userId
                };

                _context.Stalls.Add(stall);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created and stalls generated.";
            return RedirectToAction(nameof(EventDetails), new { id = eventEntity.Id });
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var userId = _userManager.GetUserId(User);
            var eventEntity = await _context.Events
                .Include(e => e.Stalls)
                .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var stallIds = eventEntity.Stalls.Select(s => s.Id).ToArray();
            var bookings = stallIds.Length > 0
                ? await _context.Bookings
                    .Include(b => b.Stall)
                    .Where(b => stallIds.Contains(b.StallId))
                    .ToListAsync()
                : new System.Collections.Generic.List<Booking>();

            var bookingIds = bookings.Select(b => b.Id).ToArray();

            var payments = bookingIds.Length > 0
                ? await _context.Payments
                    .Where(p => bookingIds.Contains(p.BookingId))
                    .ToListAsync()
                : new System.Collections.Generic.List<Payment>();

            var slots = eventEntity.Stalls
                .OrderBy(s => s.SlotNumber)
                .Select(s => new EventSlotViewModel
                {
                    StallId = s.Id,
                    SlotNumber = s.SlotNumber,
                    Status = s.Status,
                    Name = s.Name,
                    Description = s.Description,
                    RentPerDay = s.RentPerDay,
                    HasBookings = bookings.Any(b => b.StallId == s.Id)
                })
                .ToList();

            var model = new OwnerEventDetailsViewModel
            {
                EventId = eventEntity.Id,
                Name = eventEntity.Name,
                Location = eventEntity.Location,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                StallSize = eventEntity.StallSize,
                SlotPrice = eventEntity.SlotPrice,
                Description = eventEntity.Description,
                TotalSlots = slots.Count,
                AvailableSlots = slots.Count(s => s.Status == StallStatus.Available),
                BookedSlots = slots.Count(s => s.Status == StallStatus.Booked),
                PendingBookings = bookings.Count(b => b.PaymentStatus == PaymentStatus.Pending),
                TotalRevenue = decimal.Round(payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero),
                Slots = slots
            };

            return View(model);
        }

        public async Task<IActionResult> MyEvents()
        {
            var userId = _userManager.GetUserId(User);
            var events = await _context.Events
                .Where(e => e.OwnerId == userId)
                .Include(e => e.Stalls)
                .OrderByDescending(e => e.StartDate)
                .Select(e => new OwnerEventListItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Location = e.Location,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    SlotPrice = e.SlotPrice,
                    TotalSlots = e.Stalls.Count,
                    AvailableSlots = e.Stalls.Count(s => s.Status == StallStatus.Available)
                })
                .ToListAsync();

            return View(events);
        }

        [HttpGet]
        public IActionResult MyStalls()
        {
            return RedirectToAction(nameof(MyEvents));
        }

        [HttpGet]
        public async Task<IActionResult> CreateStall(int eventId, int? slotNumber = null)
        {
            var userId = _userManager.GetUserId(User);
            var eventEntity = await _context.Events
                .Include(e => e.Stalls)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OwnerId == userId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            SetEventContext(eventEntity);

            var nextSlot = slotNumber ?? CalculateNextSlotNumber(eventEntity);

            var model = new OwnerStallFormViewModel
            {
                EventId = eventEntity.Id,
                SlotNumber = nextSlot,
                Name = string.IsNullOrWhiteSpace(eventEntity.Name) ? $"Slot {nextSlot}" : $"{eventEntity.Name} · Slot {nextSlot}",
                Location = eventEntity.Location,
                Size = eventEntity.StallSize,
                RentPerDay = eventEntity.SlotPrice,
                Description = eventEntity.Description,
                Status = StallStatus.Available
            };

            return View("StallForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStall(OwnerStallFormViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var eventEntity = await _context.Events
                .Include(e => e.Stalls)
                .FirstOrDefaultAsync(e => e.Id == model.EventId && e.OwnerId == userId);

            if (eventEntity == null)
            {
                ModelState.AddModelError(nameof(model.EventId), "The selected event could not be found.");
            }
            else
            {
                SetEventContext(eventEntity);

                if (eventEntity.Stalls.Any(s => s.SlotNumber == model.SlotNumber))
                {
                    ModelState.AddModelError(nameof(model.SlotNumber), "This slot number already exists for the event.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("StallForm", model);
            }

            var stall = new Stall
            {
                EventId = eventEntity.Id,
                SlotNumber = model.SlotNumber,
                Name = string.IsNullOrWhiteSpace(model.Name) ? $"{eventEntity.Name} · Slot {model.SlotNumber}" : model.Name,
                Location = string.IsNullOrWhiteSpace(model.Location) ? eventEntity.Location : model.Location,
                Size = string.IsNullOrWhiteSpace(model.Size) ? eventEntity.StallSize : model.Size,
                RentPerDay = model.RentPerDay <= 0 ? eventEntity.SlotPrice : model.RentPerDay,
                Description = model.Description,
                OwnerId = userId,
                Status = model.Status
            };

            _context.Stalls.Add(stall);
            eventEntity.TotalSlots += 1;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Slot added successfully.";
            return RedirectToAction(nameof(EventDetails), new { id = eventEntity.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditStall(int id)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            SetEventContext(stall.Event);

            var model = new OwnerStallFormViewModel
            {
                Id = stall.Id,
                EventId = stall.EventId,
                SlotNumber = stall.SlotNumber,
                Name = stall.Name,
                Location = stall.Location,
                Size = stall.Size,
                RentPerDay = stall.RentPerDay,
                Description = stall.Description,
                Status = stall.Status
            };

            return View("StallForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStall(OwnerStallFormViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == model.Id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            SetEventContext(stall.Event);

            if (!ModelState.IsValid)
            {
                model.EventId = stall.EventId;
                model.SlotNumber = stall.SlotNumber;
                return View("StallForm", model);
            }

            stall.Name = model.Name;
            stall.Location = model.Location;
            stall.Size = model.Size;
            stall.RentPerDay = model.RentPerDay;
            stall.Description = model.Description;
            stall.Status = model.Status;
            stall.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Slot updated successfully.";
            return RedirectToAction(nameof(EventDetails), new { id = stall.EventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStall(int id)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            var eventId = stall.EventId;

            if (await _context.Bookings.AnyAsync(b => b.StallId == id))
            {
                TempData["Error"] = "Cannot delete a slot that has bookings.";
                return RedirectToAction(nameof(EventDetails), new { id = eventId });
            }

            _context.Stalls.Remove(stall);
            if (stall.Event != null)
            {
                stall.Event.TotalSlots = Math.Max(0, stall.Event.TotalSlots - 1);
                stall.Event.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Slot removed.";
            return RedirectToAction(nameof(EventDetails), new { id = eventId });
        }

        public async Task<IActionResult> Bookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookingEntities = await _context.Bookings
                .Include(b => b.Stall)
                .Include(b => b.Customer)
                .Where(b => b.Stall.OwnerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookingIds = bookingEntities.Select(b => b.Id).ToArray();
            var payments = bookingIds.Length > 0
                ? await _context.Payments
                    .Where(p => bookingIds.Contains(p.BookingId))
                    .ToListAsync()
                : new List<Payment>();

            var paymentLookup = payments
                .GroupBy(p => p.BookingId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.ProcessedAt).FirstOrDefault());

            var bookings = bookingEntities.Select(b =>
            {
                paymentLookup.TryGetValue(b.Id, out var payment);
                return new OwnerBookingViewModel
                {
                    Id = b.Id,
                    StallName = b.Stall != null ? b.Stall.Name : string.Empty,
                    CustomerName = b.Customer != null ? b.Customer.FullName : string.Empty,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    Amount = CalculateAmount(b.StartDate, b.EndDate, b.Stall != null ? b.Stall.RentPerDay : 0m),
                    PaymentReference = payment != null ? payment.TransactionReference : null
                };
            }).ToList();

            return View(bookings);
        }

        public async Task<IActionResult> StallDetails(int id)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            SetEventContext(stall.Event);

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Stall)
                .Where(b => b.StallId == stall.Id)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var bookingIds = bookings.Select(b => b.Id).ToArray();
            var bookingById = bookings.ToDictionary(b => b.Id);
            var payments = bookingIds.Length > 0
                ? await _context.Payments.Where(p => bookingIds.Contains(p.BookingId)).ToListAsync()
                : new List<Payment>();

            var paymentLookup = payments
                .GroupBy(p => p.BookingId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.ProcessedAt).FirstOrDefault());

            var feedbackEntries = bookingIds.Length > 0
                ? await _context.Feedback
                    .Where(f => bookingIds.Contains(f.BookingId))
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToListAsync()
                : new List<Feedback>();

            var bookingDetails = bookings
                .Select(b =>
                {
                    paymentLookup.TryGetValue(b.Id, out var payment);
                    return MapBookingDetail(b, payment);
                })
                .ToList();

            var feedbackModels = feedbackEntries
                .Select(f =>
                {
                    bookingById.TryGetValue(f.BookingId, out var booking);
                    return new OwnerFeedbackViewModel
                    {
                        StallName = stall.Name,
                        CustomerName = booking != null && booking.Customer != null ? booking.Customer.FullName : string.Empty,
                        Rating = f.Rating,
                        Comments = f.Comments,
                        SubmittedAt = f.SubmittedAt
                    };
                })
                .ToList();

            var nextBooking = bookings
                .Where(b => b.Status == BookingStatus.Approved && b.EndDate >= DateTime.UtcNow.Date)
                .OrderBy(b => b.StartDate)
                .FirstOrDefault();

            OwnerBookingDetailViewModel nextBookingModel = null;
            if (nextBooking != null)
            {
                paymentLookup.TryGetValue(nextBooking.Id, out var payment);
                nextBookingModel = MapBookingDetail(nextBooking, payment);
            }

            double? averageRating = null;
            var feedbackRatings = feedbackEntries
                .Where(f => f.Rating.HasValue)
                .Select(f => (double)f.Rating.Value)
                .ToList();

            if (feedbackRatings.Any())
            {
                averageRating = Math.Round(feedbackRatings.Average(), 1);
            }

            var stallRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);

            var model = new OwnerStallDetailViewModel
            {
                StallId = stall.Id,
                SlotNumber = stall.SlotNumber,
                Name = stall.Name,
                Location = stall.Location,
                Size = stall.Size,
                RentPerDay = stall.RentPerDay,
                Status = stall.Status,
                EventId = stall.EventId,
                EventName = stall.Event?.Name,
                TotalBookings = bookings.Count,
                PendingRequests = bookings.Count(b => b.Status == BookingStatus.Pending),
                TotalRevenue = decimal.Round(stallRevenue, 2, MidpointRounding.AwayFromZero),
                AverageRating = averageRating,
                ReviewCount = feedbackEntries.Count,
                NextBooking = nextBookingModel,
                Description = stall.Description,
                CreatedAt = stall.CreatedAt,
                UpdatedAt = stall.UpdatedAt,
                BookingHistory = bookingDetails,
                Feedback = feedbackModels
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == id && b.Stall.OwnerId == userId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = BookingStatus.Approved;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Stall.Status = StallStatus.Booked;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking approved.";
            return RedirectToAction(nameof(Bookings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == id && b.Stall.OwnerId == userId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = BookingStatus.Rejected;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Stall.Status = StallStatus.Available;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking rejected.";
            return RedirectToAction(nameof(Bookings));
        }

        public async Task<IActionResult> Feedback()
        {
            var userId = _userManager.GetUserId(User);
            var feedback = await _context.Feedback
                .Include(f => f.Booking)
                .ThenInclude(b => b.Stall)
                .Include(f => f.Booking)
                .ThenInclude(b => b.Customer)
                .Where(f => f.Booking.Stall.OwnerId == userId)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new OwnerFeedbackViewModel
                {
                    StallName = f.Booking.Stall.Name,
                    CustomerName = f.Booking.Customer.FullName,
                    Rating = f.Rating,
                    Comments = f.Comments,
                    SubmittedAt = f.SubmittedAt
                }).ToListAsync();

            return View(feedback);
        }

        private async Task<StallOwnerDashboardViewModel> BuildDashboardModelAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var stalls = await _context.Stalls
                .Where(s => s.OwnerId == userId)
                .Include(s => s.Event)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            var stallIds = stalls.Select(s => s.Id).ToArray();

            var bookings = new List<Booking>();
            var payments = new List<Payment>();
            var feedbackEntries = new List<Feedback>();

            if (stallIds.Length > 0)
            {
                bookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Stall)
                    .Where(b => stallIds.Contains(b.StallId))
                    .AsNoTracking()
                    .ToListAsync();

                var bookingIds = bookings.Select(b => b.Id).ToArray();

                if (bookingIds.Length > 0)
                {
                    payments = await _context.Payments
                        .Where(p => bookingIds.Contains(p.BookingId))
                        .AsNoTracking()
                        .ToListAsync();

                    feedbackEntries = await _context.Feedback
                        .Where(f => bookingIds.Contains(f.BookingId))
                        .OrderByDescending(f => f.SubmittedAt)
                        .AsNoTracking()
                        .ToListAsync();
                }
            }

            var bookingLookup = bookings.ToDictionary(b => b.Id);
            var paymentLookup = payments
                .GroupBy(p => p.BookingId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.ProcessedAt).FirstOrDefault());

            var revenueByStall = payments
                .Where(p => p.Status == PaymentStatus.Completed && bookingLookup.ContainsKey(p.BookingId))
                .GroupBy(p => bookingLookup[p.BookingId].StallId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var reviewsByStall = feedbackEntries
                .Where(f => bookingLookup.ContainsKey(f.BookingId))
                .GroupBy(f => bookingLookup[f.BookingId].StallId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var stallSummaries = stalls.Select(stall =>
            {
                var stallBookings = bookings.Where(b => b.StallId == stall.Id).ToList();
                var stallReviews = reviewsByStall.TryGetValue(stall.Id, out var reviewList)
                    ? reviewList
                    : new List<Feedback>();

                var nextBookingEntity = stallBookings
                    .Where(b => b.Status == BookingStatus.Approved && b.EndDate >= today)
                    .OrderBy(b => b.StartDate)
                    .FirstOrDefault();

                OwnerBookingDetailViewModel nextBooking = null;
                if (nextBookingEntity != null)
                {
                    paymentLookup.TryGetValue(nextBookingEntity.Id, out var payment);
                    nextBooking = MapBookingDetail(nextBookingEntity, payment);
                }

                double? averageRating = null;
                var reviewRatings = stallReviews
                    .Where(r => r.Rating.HasValue)
                    .Select(r => (double)r.Rating.Value)
                    .ToList();

                if (reviewRatings.Any())
                {
                    averageRating = Math.Round(reviewRatings.Average(), 1);
                }

                var stallRevenue = revenueByStall.TryGetValue(stall.Id, out var revenue)
                    ? revenue
                    : 0m;

                return new OwnerStallSummaryViewModel
                {
                    StallId = stall.Id,
                    EventId = stall.EventId,
                    EventName = stall.Event?.Name,
                    SlotNumber = stall.SlotNumber,
                    Name = stall.Name,
                    Location = stall.Location,
                    Size = stall.Size,
                    RentPerDay = stall.RentPerDay,
                    Status = stall.Status,
                    TotalBookings = stallBookings.Count,
                    PendingRequests = stallBookings.Count(b => b.Status == BookingStatus.Pending),
                    TotalRevenue = decimal.Round(stallRevenue, 2, MidpointRounding.AwayFromZero),
                    AverageRating = averageRating,
                    ReviewCount = stallReviews.Count,
                    NextBooking = nextBooking
                };
            }).ToList();

            var upcomingBookings = bookings
                .Where(b => b.Status == BookingStatus.Approved && b.EndDate >= today)
                .OrderBy(b => b.StartDate)
                .Take(6)
                .Select(b =>
                {
                    paymentLookup.TryGetValue(b.Id, out var payment);
                    return MapBookingDetail(b, payment);
                })
                .ToList();

            var recentFeedback = feedbackEntries
                .Take(6)
                .Select(f =>
                {
                    var booking = bookingLookup[f.BookingId];
                    return new OwnerFeedbackViewModel
                    {
                        StallName = booking.Stall.Name,
                        CustomerName = booking.Customer != null ? booking.Customer.FullName : "-",
                        Rating = f.Rating,
                        Comments = f.Comments,
                        SubmittedAt = f.SubmittedAt
                    };
                })
                .ToList();

            var totalRevenue = revenueByStall.Values.DefaultIfEmpty(0m).Sum();

            return new StallOwnerDashboardViewModel
            {
                MyStallCount = stalls.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                UpcomingBookings = upcomingBookings.Count,
                TotalRevenue = decimal.Round(totalRevenue, 2, MidpointRounding.AwayFromZero),
                StallSummaries = stallSummaries,
                UpcomingBookingDetails = upcomingBookings,
                RecentFeedback = recentFeedback
            };
        }

        private void SetEventContext(Event eventEntity)
        {
            if (eventEntity == null)
            {
                return;
            }

            ViewData["EventName"] = eventEntity.Name;
            ViewData["EventLocation"] = eventEntity.Location;
            ViewData["EventDates"] = $"{eventEntity.StartDate:dd MMM yyyy} - {eventEntity.EndDate:dd MMM yyyy}";
        }

        private static int CalculateNextSlotNumber(Event eventEntity)
        {
            if (eventEntity?.Stalls == null || !eventEntity.Stalls.Any())
            {
                return 1;
            }

            var orderedSlots = eventEntity.Stalls
                .Select(s => s.SlotNumber)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToArray();

            var expected = 1;
            foreach (var slot in orderedSlots)
            {
                if (slot > expected)
                {
                    break;
                }

                if (slot == expected)
                {
                    expected++;
                }
            }

            return expected;
        }

        private static OwnerBookingDetailViewModel MapBookingDetail(Booking booking, Payment payment = null)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            var stallName = booking.Stall != null ? booking.Stall.Name : string.Empty;
            var customerName = booking.Customer != null ? booking.Customer.FullName : string.Empty;
            var rentPerDay = booking.Stall != null ? booking.Stall.RentPerDay : 0m;

            return new OwnerBookingDetailViewModel
            {
                BookingId = booking.Id,
                StallName = stallName,
                CustomerName = customerName,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                Amount = CalculateAmount(booking.StartDate, booking.EndDate, rentPerDay),
                PaymentReference = payment != null ? payment.TransactionReference : null,
                PaymentDate = payment?.ProcessedAt
            };
        }

        private static decimal CalculateAmount(DateTime start, DateTime end, decimal rentPerDay)
        {
            var days = (end.Date - start.Date).Days + 1;
            return Math.Max(days, 1) * rentPerDay;
        }
    }
}
