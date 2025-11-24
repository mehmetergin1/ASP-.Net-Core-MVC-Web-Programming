using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicRequestPortal.Data;
using CivicRequestPortal.Models;
using CivicRequestPortal.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace CivicRequestPortal.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _configuration;

    public AdminController(ApplicationDbContext context, EmailService emailService, ILogger<AdminController> logger, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    private bool IsAdminAuthenticated()
    {
        try
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
        catch
        {
            return false;
        }
    }

    // GET: Admin
    public async Task<IActionResult> Index(string statusFilter = "all", string categoryFilter = "all")
    {
        if (!IsAdminAuthenticated())
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Index") });
        }
        var query = _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Status)
            .Include(r => r.Assignments)
                .ThenInclude(a => a.AssignedToUser)
            .AsQueryable();

        if (statusFilter != "all")
        {
            if (int.TryParse(statusFilter, out int statusId))
            {
                query = query.Where(r => r.StatusId == statusId);
            }
        }

        if (categoryFilter != "all")
        {
            if (int.TryParse(categoryFilter, out int categoryId))
            {
                query = query.Where(r => r.CategoryId == categoryId);
            }
        }

        var requests = await query
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        ViewBag.Statuses = await _context.RequestStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Users = await _context.Users
            .Where(u => u.UserType == "Admin" && u.IsActive)
            .ToListAsync();

        ViewBag.StatusFilter = statusFilter;
        ViewBag.CategoryFilter = categoryFilter;

        return View(requests);
    }

    // GET: Admin/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!IsAdminAuthenticated())
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Details", new { id }) });
        }
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

        ViewBag.Statuses = await _context.RequestStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        ViewBag.Users = await _context.Users
            .Where(u => (u.UserType == "Admin" || u.UserType == "MunicipalityAdmin") && u.IsActive)
            .ToListAsync();

        return View(request);
    }

    // POST: Admin/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int requestId, int statusId, string? comment)
    {
        if (!IsAdminAuthenticated())
        {
            TempData["ErrorMessage"] = "Yetkili değilsiniz.";
            return RedirectToAction("Login", new { returnUrl = Url.Action("Details", new { id = requestId }) });
        }
        var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);

        if (request == null)
        {
            return NotFound();
        }

        var oldStatus = request.Status?.Name;
        request.StatusId = statusId;

        // Update timestamps based on status
        if (statusId == 3) // Assigned
        {
            request.AssignedAt = DateTime.Now;
        }
        else if (statusId == 4) // Resolved
        {
            request.ResolvedAt = DateTime.Now;
        }
        else if (statusId == 5) // Closed
        {
            request.ClosedAt = DateTime.Now;
        }

        // Check SLA
        if (request.SLADeadline.HasValue && DateTime.Now > request.SLADeadline.Value && statusId != 4 && statusId != 5)
        {
            request.IsSLABreached = true;
        }

        // Add update comment
        if (!string.IsNullOrEmpty(comment))
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserType == "Admin");
            if (adminUser != null)
            {
                var update = new RequestUpdate
                {
                    RequestId = requestId,
                    UserId = adminUser.UserId,
                    Comment = comment,
                    UpdateType = "StatusChange",
                    CreatedAt = DateTime.Now,
                    IsInternal = false
                };
                _context.RequestUpdates.Add(update);
            }
        }

        await _context.SaveChangesAsync();

        // Send email notification to citizen
        var newStatus = await _context.RequestStatuses.FindAsync(statusId);
        if (newStatus != null && request.User != null)
        {
            await _emailService.SendStatusUpdateEmailAsync(
                request.User.Email, 
                request.RequestNumber, 
                request.Title, 
                newStatus.Name);
        }

        TempData["SuccessMessage"] = "Durum başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { id = requestId });
    }

    // POST: Admin/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int requestId, int assignedToUserId, string? notes)
    {
        if (!IsAdminAuthenticated())
        {
            TempData["ErrorMessage"] = "Yetkili değilsiniz.";
            return RedirectToAction("Login", new { returnUrl = Url.Action("Details", new { id = requestId }) });
        }
        // Deactivate previous assignments
        var previousAssignments = await _context.RequestAssignments
            .Where(a => a.RequestId == requestId && a.IsActive)
            .ToListAsync();

        foreach (var assignment in previousAssignments)
        {
            assignment.IsActive = false;
        }

        // Create new assignment
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserType == "Admin");
        var newAssignment = new RequestAssignment
        {
            RequestId = requestId,
            AssignedToUserId = assignedToUserId,
            AssignedByUserId = adminUser?.UserId,
            Notes = notes,
            AssignedAt = DateTime.Now,
            IsActive = true
        };

        _context.RequestAssignments.Add(newAssignment);

        // Update request status to Assigned
        var request = await _context.ServiceRequests.FindAsync(requestId);
        if (request != null)
        {
            request.StatusId = 3; // Assigned
            request.AssignedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Görev başarıyla atandı.";
        return RedirectToAction(nameof(Details), new { id = requestId });
    }

    // POST: Admin/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int requestId, string comment, bool isInternal = false)
    {
        if (!IsAdminAuthenticated())
        {
            TempData["ErrorMessage"] = "Yetkili değilsiniz.";
            return RedirectToAction("Login", new { returnUrl = Url.Action("Details", new { id = requestId }) });
        }
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserType == "Admin");
        if (adminUser == null)
        {
            TempData["ErrorMessage"] = "Admin kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Details), new { id = requestId });
        }

        var update = new RequestUpdate
        {
            RequestId = requestId,
            UserId = adminUser.UserId,
            Comment = comment,
            UpdateType = "Comment",
            CreatedAt = DateTime.Now,
            IsInternal = isInternal
        };

        _context.RequestUpdates.Add(update);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Yorum başarıyla eklendi.";
        return RedirectToAction(nameof(Details), new { id = requestId });
    }

    // GET: Admin/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: Admin/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string password, string? returnUrl = null)
    {
        var configured = _configuration.GetValue<string>("AdminAuth:Password") ?? string.Empty;
        if (!string.IsNullOrEmpty(password) && password == configured)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        ViewBag.ReturnUrl = returnUrl;
        ModelState.AddModelError("", "Yetkili değilsiniz.");
        return View();
    }

    // GET: Admin/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsAdmin");
        return RedirectToAction("Login");
    }
}

