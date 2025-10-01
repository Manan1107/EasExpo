using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EasExpoDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            EasExpoDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel
            {
                UserType = RoleNames.Customer
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.UserType != RoleNames.Customer && model.UserType != RoleNames.StallOwner)
            {
                ModelState.AddModelError(nameof(model.UserType), "Please choose a valid user type.");
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                CompanyName = model.CompanyName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var isStallOwner = model.UserType == RoleNames.StallOwner;
            if (!isStallOwner)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Customer);
            }

            if (isStallOwner)
            {
                _context.StallOwnerApplications.Add(new StallOwnerApplication
                {
                    UserId = user.Id,
                    DocumentUrl = model.DocumentUrl,
                    AdditionalNotes = model.AdditionalNotes
                });
                await _context.SaveChangesAsync();

                TempData["Success"] = "Application submitted. Our team will review your details and contact you shortly.";
                return RedirectToAction(nameof(OwnerApplicationSubmitted));
            }

            TempData["Success"] = "Registration successful. Welcome aboard!";
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Events");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials or inactive account.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(RoleNames.Admin))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                if (roles.Contains(RoleNames.StallOwner))
                {
                    return RedirectToAction("Dashboard", "StallOwner");
                }

                var application = await _context.StallOwnerApplications
                    .OrderByDescending(a => a.SubmittedAt)
                    .FirstOrDefaultAsync(a => a.UserId == user.Id);

                if (application != null && application.Status != ApplicationStatus.Approved)
                {
                    TempData["Info"] = application.Status == ApplicationStatus.Pending
                        ? "Your stall owner application is still under review. We'll notify you once it's approved."
                        : "Your stall owner application was reviewed. See the latest status below.";
                    return RedirectToAction(nameof(OwnerApplicationStatus));
                }

                return RedirectToAction("Index", "Events");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult OwnerApplicationSubmitted()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OwnerApplicationStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (await _userManager.IsInRoleAsync(user, RoleNames.StallOwner))
            {
                return RedirectToAction("Dashboard", "StallOwner");
            }

            var application = await _context.StallOwnerApplications
                .Include(a => a.User)
                .OrderByDescending(a => a.SubmittedAt)
                .FirstOrDefaultAsync(a => a.UserId == user.Id);

            var model = new OwnerApplicationStatusViewModel
            {
                HasApplication = application != null,
                Status = application?.Status,
                SubmittedAt = application?.SubmittedAt,
                ReviewedAt = application?.ReviewedAt,
                ReviewedBy = application?.ReviewedBy,
                DocumentUrl = application?.DocumentUrl,
                AdditionalNotes = application?.AdditionalNotes
            };

            if (application == null)
            {
                TempData["Error"] = "We couldn't find a stall owner application associated with your account. Please contact support or submit a new application.";
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new AccountProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                CompanyName = user.CompanyName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
