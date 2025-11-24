using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicRequestPortal.Data;
using CivicRequestPortal.Models;
using CivicRequestPortal.Models.ViewModels;
using CivicRequestPortal.Services;

namespace CivicRequestPortal.Controllers;

public class RequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(ApplicationDbContext context, EmailService emailService, ILogger<RequestsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // GET: Requests/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        
        // Municipalities removed from the application.

        return View();
    }

    // POST: Requests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Prefer invariant-formatted lat/lng from hidden inputs if present (reliable across cultures)
            try
            {
                var latInv = Request.Form["LatitudeInvariant"].FirstOrDefault();
                var lngInv = Request.Form["LongitudeInvariant"].FirstOrDefault();
                if (!string.IsNullOrEmpty(latInv))
                {
                    model.Latitude = decimal.Parse(latInv, System.Globalization.CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(lngInv))
                {
                    model.Longitude = decimal.Parse(lngInv, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Latitude/Longitude parsing failed for invariant values.");
            }
            // Generate unique request number
            var requestNumber = $"REQ-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Get or create user from submitted form (if you have authentication, replace this with current user)
            User? user = null;
            if (!string.IsNullOrEmpty(model.Email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            }

            if (user == null)
            {
                user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    UserType = "Citizen",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update contact info if changed
                var updated = false;
                if (!string.IsNullOrEmpty(model.FirstName) && user.FirstName != model.FirstName) { user.FirstName = model.FirstName; updated = true; }
                if (!string.IsNullOrEmpty(model.LastName) && user.LastName != model.LastName) { user.LastName = model.LastName; updated = true; }
                if (!string.IsNullOrEmpty(model.PhoneNumber) && user.PhoneNumber != model.PhoneNumber) { user.PhoneNumber = model.PhoneNumber; updated = true; }
                if (updated)
                {
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            var category = await _context.Categories.FindAsync(model.CategoryId);
            var defaultSLA = category?.DefaultSLAHours ?? 72;

            var request = new ServiceRequest
            {
                RequestNumber = requestNumber,
                Title = model.Title,
                Description = model.Description,
                UserId = user.UserId,
                CategoryId = model.CategoryId,
                StatusId = 1, // Submitted
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Priority = model.Priority,
                SubmittedAt = DateTime.Now,
                SLAHours = defaultSLA,
                SLADeadline = DateTime.Now.AddHours(defaultSLA),
                IsSLABreached = false
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Send email notification
            await _emailService.SendRequestSubmittedEmailAsync(user.Email, requestNumber, model.Title);

            TempData["SuccessMessage"] = $"Şikayetiniz başarıyla kaydedildi. Şikayet numaranız: {requestNumber}";
            return RedirectToAction(nameof(Track), new { requestNumber });
        }

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(model);
    }

    // GET: Requests/Track
    public IActionResult Track()
    {
        return View();
    }

    // POST: Requests/Track
    [HttpPost]
    public async Task<IActionResult> Track(string requestNumber)
    {
        if (string.IsNullOrEmpty(requestNumber))
        {
            ModelState.AddModelError("", "Lütfen bir şikayet numarası giriniz.");
            return View();
        }

            var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Status)
            .Include(r => r.Updates)
                .ThenInclude(u => u.User)
            .Include(r => r.Assignments)
                .ThenInclude(a => a.AssignedToUser)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber);

        if (request == null)
        {
            ModelState.AddModelError("", "Belirtilen şikayet numarası bulunamadı.");
            return View();
        }

        return View("Details", request);
    }

    // GET: Requests/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Status)
            .Include(r => r.Updates)
                .ThenInclude(u => u.User)
            .Include(r => r.Assignments)
                .ThenInclude(a => a.AssignedToUser)
            .FirstOrDefaultAsync(m => m.RequestId == id);

        if (request == null)
        {
            return NotFound();
        }

        return View(request);
    }
}

