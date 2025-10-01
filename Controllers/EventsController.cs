using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [AllowAnonymous]
    public class EventsController : Controller
    {
        private readonly EasExpoDbContext _context;

        public EventsController(EasExpoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search = null)
        {
            var query = _context.Events
                .Include(e => e.Stalls)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e => e.Name.Contains(term) || e.Location.Contains(term));
            }

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new EventListItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Location = e.Location,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StallSize = e.StallSize,
                    SlotPrice = e.SlotPrice,
                    TotalSlots = e.Stalls.Count,
                    AvailableSlots = e.Stalls.Count(s => s.Status == StallStatus.Available)
                }).ToListAsync();

            ViewData["Search"] = search;
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Stalls)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var slots = eventEntity.Stalls
                .OrderBy(s => s.SlotNumber)
                .Select(s => new EventSlotViewModel
                {
                    StallId = s.Id,
                    SlotNumber = s.SlotNumber,
                    Status = s.Status,
                    Name = s.Name,
                    Description = s.Description,
                    RentPerDay = s.RentPerDay
                }).ToList();

            var model = new EventDetailsViewModel
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Location = eventEntity.Location,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                StallSize = eventEntity.StallSize,
                SlotPrice = eventEntity.SlotPrice,
                Description = eventEntity.Description,
                TotalSlots = slots.Count,
                AvailableSlots = slots.Count(s => s.Status == StallStatus.Available),
                Slots = slots,
                CanBook = User.Identity.IsAuthenticated && User.IsInRole(RoleNames.Customer)
            };

            return View(model);
        }
    }
}
