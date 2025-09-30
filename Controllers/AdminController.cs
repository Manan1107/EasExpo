using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.Admin;
using EasExpo.Models.ViewModels.Stalls;
using EasExpo.Models.ViewModels.StallOwner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController : Controller
    {
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(EasExpoDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var customers = await _userManager.GetUsersInRoleAsync(RoleNames.Customer);
            var owners = await _userManager.GetUsersInRoleAsync(RoleNames.StallOwner);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveCustomers = customers.Count(u => u.IsActive),
                ActiveOwners = owners.Count(u => u.IsActive),
                TotalStalls = await _context.Stalls.CountAsync(),
                AvailableStalls = await _context.Stalls.CountAsync(s => s.Status == StallStatus.Available),
                PendingOwnerApplications = await _context.StallOwnerApplications.CountAsync(a => a.Status == ApplicationStatus.Pending),
                PendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                TotalRevenue = Math.Round(totalRevenue, 2),
                FailedPayments = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Failed),
                TotalPayments = await _context.Payments.CountAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();

            var viewModel = await Task.WhenAll(users.Select(async user =>
            {
                var roles = await _userManager.GetRolesAsync(user);
                return new AdminUserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "-",
                    IsActive = user.IsActive
                };
            }));

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View("UserForm", new AdminUserFormViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminUserFormViewModel model)
        {
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (!ModelState.IsValid)
            {
                return View("UserForm", model);
            }

            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError(nameof(model.Password), "Please supply a password with at least 8 characters.");
                return View("UserForm", model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                CompanyName = model.CompanyName,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("UserForm", model);
            }

            if (!string.IsNullOrWhiteSpace(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            if (model.Role == RoleNames.StallOwner)
            {
                _context.StallOwnerApplications.Add(new StallOwnerApplication
                {
                    UserId = user.Id,
                    Status = ApplicationStatus.Approved,
                    ReviewedAt = DateTime.UtcNow,
                    ReviewedBy = User.Identity.Name
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new AdminUserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CompanyName = user.CompanyName,
                Role = roles.FirstOrDefault(),
                IsActive = user.IsActive,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View("UserForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AdminUserFormViewModel model)
        {
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            if (!ModelState.IsValid)
            {
                return View("UserForm", model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.CompanyName = model.CompanyName;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("UserForm", model);
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
            }

            if (!string.IsNullOrWhiteSpace(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Email == User.Identity.Name)
            {
                TempData["Error"] = "You cannot delete the currently logged in admin.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User removed.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Stalls()
        {
            var stalls = await _context.Stalls
                .Include(s => s.Owner)
                .OrderBy(s => s.Name)
                .Select(s => new StallListItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location,
                    Size = s.Size,
                    RentPerDay = s.RentPerDay,
                    OwnerName = s.Owner.FullName,
                    Status = s.Status
                }).ToListAsync();

            return View(stalls);
        }

        public async Task<IActionResult> StallDetails(int id)
        {
            var stall = await _context.Stalls
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stall == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Where(b => b.StallId == stall.Id)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var bookingIds = bookings.Select(b => b.Id).ToArray();
            var bookingById = bookings.ToDictionary(b => b.Id);

            var payments = bookingIds.Length > 0
                ? await _context.Payments
                    .Where(p => bookingIds.Contains(p.BookingId))
                    .OrderByDescending(p => p.ProcessedAt)
                    .ToListAsync()
                : new List<Payment>();

            var feedbackEntries = bookingIds.Length > 0
                ? await _context.Feedback
                    .Where(f => bookingIds.Contains(f.BookingId))
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToListAsync()
                : new List<Feedback>();

            var paymentLookup = payments
                .GroupBy(p => p.BookingId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderByDescending(p => p.Status == PaymentStatus.Completed)
                        .ThenByDescending(p => p.ProcessedAt)
                        .ToList());

            var bookingHistory = bookings.Select(b =>
            {
                paymentLookup.TryGetValue(b.Id, out var bookingPayments);
                var paymentList = bookingPayments ?? new List<Payment>();
                var completedPayment = paymentList.FirstOrDefault(p => p.Status == PaymentStatus.Completed);
                var preferredPayment = completedPayment ?? paymentList.FirstOrDefault();

                return new AdminStallBookingDetailViewModel
                {
                    BookingId = b.Id,
                    CustomerName = b.Customer?.FullName ?? "-",
                    CustomerEmail = b.Customer?.Email,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    Amount = CalculateAmount(b.StartDate, b.EndDate, stall.RentPerDay),
                    PaymentReference = preferredPayment?.TransactionReference,
                    PaymentDate = preferredPayment?.ProcessedAt,
                    PaymentAmount = preferredPayment?.Amount,
                    PaymentProvider = preferredPayment?.Provider
                };
            }).ToList();

            var totalRevenue = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            var averageRating = feedbackEntries.Any()
                ? Math.Round(feedbackEntries.Average(f => f.Rating), 1)
                : (double?)null;

            var feedbackModels = feedbackEntries
                .Select(f =>
                {
                    bookingById.TryGetValue(f.BookingId, out var booking);
                    return new OwnerFeedbackViewModel
                    {
                        StallName = stall.Name,
                        CustomerName = booking?.Customer?.FullName ?? "-",
                        Rating = f.Rating,
                        Comments = f.Comments,
                        SubmittedAt = f.SubmittedAt
                    };
                })
                .ToList();

            var model = new AdminStallDetailViewModel
            {
                StallId = stall.Id,
                Name = stall.Name,
                Location = stall.Location,
                Size = stall.Size,
                Description = stall.Description,
                RentPerDay = stall.RentPerDay,
                Status = stall.Status,
                CreatedAt = stall.CreatedAt,
                UpdatedAt = stall.UpdatedAt,
                OwnerName = stall.Owner?.FullName,
                OwnerEmail = stall.Owner?.Email,
                OwnerPhone = stall.Owner?.PhoneNumber,
                OwnerCompany = stall.Owner?.CompanyName,
                TotalBookings = bookings.Count,
                PendingRequests = bookings.Count(b => b.Status == BookingStatus.Pending),
                TotalRevenue = decimal.Round(totalRevenue, 2, MidpointRounding.AwayFromZero),
                AverageRating = averageRating,
                ReviewCount = feedbackEntries.Count,
                BookingHistory = bookingHistory,
                Feedback = feedbackModels
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStall()
        {
            var owners = await _userManager.GetUsersInRoleAsync(RoleNames.StallOwner);
            return View("StallForm", new StallFormViewModel
            {
                AvailableOwners = owners
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStall(StallFormViewModel model)
        {
            var owners = await _userManager.GetUsersInRoleAsync(RoleNames.StallOwner);
            model.AvailableOwners = owners;

            if (!ModelState.IsValid)
            {
                return View("StallForm", model);
            }

            if (string.IsNullOrEmpty(model.OwnerId))
            {
                ModelState.AddModelError(nameof(model.OwnerId), "Please choose an owner for this stall.");
                return View("StallForm", model);
            }

            var stall = new Stall
            {
                Name = model.Name,
                Location = model.Location,
                Size = model.Size,
                RentPerDay = model.RentPerDay,
                Description = model.Description,
                OwnerId = model.OwnerId,
                Status = model.Status
            };

            _context.Stalls.Add(stall);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall created successfully.";
            return RedirectToAction(nameof(Stalls));
        }

        [HttpGet]
        public async Task<IActionResult> EditStall(int id)
        {
            var stall = await _context.Stalls.FindAsync(id);
            if (stall == null)
            {
                return NotFound();
            }

            var owners = await _userManager.GetUsersInRoleAsync(RoleNames.StallOwner);
            var model = new StallFormViewModel
            {
                Id = stall.Id,
                Name = stall.Name,
                Location = stall.Location,
                Size = stall.Size,
                RentPerDay = stall.RentPerDay,
                Description = stall.Description,
                OwnerId = stall.OwnerId,
                Status = stall.Status,
                AvailableOwners = owners
            };

            return View("StallForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStall(StallFormViewModel model)
        {
            var owners = await _userManager.GetUsersInRoleAsync(RoleNames.StallOwner);
            model.AvailableOwners = owners;

            if (!ModelState.IsValid)
            {
                return View("StallForm", model);
            }

            if (string.IsNullOrEmpty(model.OwnerId))
            {
                ModelState.AddModelError(nameof(model.OwnerId), "Please choose an owner for this stall.");
                return View("StallForm", model);
            }

            var stall = await _context.Stalls.FindAsync(model.Id);
            if (stall == null)
            {
                return NotFound();
            }

            stall.Name = model.Name;
            stall.Location = model.Location;
            stall.Size = model.Size;
            stall.RentPerDay = model.RentPerDay;
            stall.Description = model.Description;
            stall.OwnerId = model.OwnerId;
            stall.Status = model.Status;
            stall.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall updated successfully.";
            return RedirectToAction(nameof(Stalls));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStall(int id)
        {
            var stall = await _context.Stalls.FindAsync(id);
            if (stall == null)
            {
                return NotFound();
            }

            if (await _context.Bookings.AnyAsync(b => b.StallId == id))
            {
                TempData["Error"] = "Cannot delete a stall that has bookings.";
                return RedirectToAction(nameof(Stalls));
            }

            _context.Stalls.Remove(stall);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall deleted.";
            return RedirectToAction(nameof(Stalls));
        }

        private static decimal CalculateAmount(DateTime start, DateTime end, decimal rentPerDay)
        {
            var totalDays = (end.Date - start.Date).TotalDays;
            if (totalDays < 1)
            {
                totalDays = 1;
            }

            var amount = rentPerDay * (decimal)totalDays;
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<IActionResult> OwnerApplications()
        {
            var applications = await _context.StallOwnerApplications
                .Include(a => a.User)
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new OwnerApplicationViewModel
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    ApplicantName = a.User.FullName,
                    Email = a.User.Email,
                    CompanyName = a.User.CompanyName,
                    DocumentUrl = a.DocumentUrl,
                    AdditionalNotes = a.AdditionalNotes,
                    Status = a.Status,
                    SubmittedAt = a.SubmittedAt
                }).ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            var application = await _context.StallOwnerApplications.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = ApplicationStatus.Approved;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = User.Identity.Name;

            await _context.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(application.User, RoleNames.StallOwner))
            {
                await _userManager.AddToRoleAsync(application.User, RoleNames.StallOwner);
            }

            TempData["Success"] = "Application approved.";
            return RedirectToAction(nameof(OwnerApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id)
        {
            var application = await _context.StallOwnerApplications.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = ApplicationStatus.Rejected;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = User.Identity.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(OwnerApplications));
        }

        public async Task<IActionResult> PaymentReports()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Stall)
                .ThenInclude(s => s.Owner)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
                .OrderByDescending(p => p.ProcessedAt)
                .Select(p => new PaymentReportItemViewModel
                {
                    Id = p.Id,
                    StallName = p.Booking.Stall.Name,
                    CustomerName = p.Booking.Customer.FullName,
                    Amount = p.Amount,
                    Provider = p.Provider,
                    TransactionReference = p.TransactionReference,
                    Status = p.Status,
                    ProcessedAt = p.ProcessedAt
                }).ToListAsync();

            return View(payments);
        }
    }
}
