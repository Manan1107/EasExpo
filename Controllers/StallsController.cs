using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.Stalls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [AllowAnonymous]
    public class StallsController : Controller
    {
        private readonly EasExpoDbContext _context;

        public StallsController(EasExpoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "Available", string search = null)
        {
            var query = _context.Stalls.Include(s => s.Owner).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search) || s.Location.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status) && System.Enum.TryParse(status, out StallStatus parsedStatus))
            {
                query = query.Where(s => s.Status == parsedStatus);
            }

            var stalls = await query
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

            ViewData["SelectedStatus"] = status;
            ViewData["Search"] = search;
            return View(stalls);
        }

        public async Task<IActionResult> Details(int id)
        {
            var stall = await _context.Stalls
                .Include(s => s.Owner)
                .Where(s => s.Id == id)
                .Select(s => new StallDetailsViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location,
                    Size = s.Size,
                    RentPerDay = s.RentPerDay,
                    Description = s.Description,
                    Status = s.Status,
                    OwnerName = s.Owner.FullName
                }).FirstOrDefaultAsync();

            if (stall == null)
            {
                return NotFound();
            }

            ViewBag.CanBook = User.Identity.IsAuthenticated && User.IsInRole(RoleNames.Customer);
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            return View(stall);
        }
    }
}
