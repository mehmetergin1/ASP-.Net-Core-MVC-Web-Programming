using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicRequestPortal.Data;
using CivicRequestPortal.Models.ViewModels;

namespace CivicRequestPortal.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Dashboard/Public
    public async Task<IActionResult> Public()
    {
        var model = new DashboardViewModel();

        var requests = await _context.ServiceRequests
            .Include(r => r.Category)
            .Include(r => r.Status)
            .ToListAsync();

        model.TotalRequests = requests.Count;
        model.PendingRequests = requests.Count(r => r.StatusId == 1);
        model.InProgressRequests = requests.Count(r => r.StatusId == 2 || r.StatusId == 3);
        model.ResolvedRequests = requests.Count(r => r.StatusId == 4);
        model.ClosedRequests = requests.Count(r => r.StatusId == 5);
        model.SLABreachedRequests = requests.Count(r => r.IsSLABreached);

        // Calculate average resolution time
        var resolvedRequests = requests.Where(r => r.ResolvedAt.HasValue).ToList();
        if (resolvedRequests.Any())
        {
            model.AverageResolutionTime = resolvedRequests
                .Average(r => (r.ResolvedAt!.Value - r.SubmittedAt).TotalHours);
        }

        // Calculate SLA achievement rate
        var requestsWithSLA = requests.Where(r => r.SLADeadline.HasValue).ToList();
        if (requestsWithSLA.Any())
        {
            var achievedSLA = requestsWithSLA.Count(r => !r.IsSLABreached || (r.ResolvedAt.HasValue && r.ResolvedAt <= r.SLADeadline));
            model.SLAAchievementRate = (double)achievedSLA / requestsWithSLA.Count * 100;
        }

        // Category statistics
        var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        foreach (var category in categories)
        {
            var categoryRequests = requests.Where(r => r.CategoryId == category.CategoryId).ToList();
            var categoryResolved = categoryRequests.Where(r => r.ResolvedAt.HasValue).ToList();
            
            var avgTime = categoryResolved.Any() 
                ? categoryResolved.Average(r => (r.ResolvedAt!.Value - r.SubmittedAt).TotalHours)
                : 0;

            model.CategoryStatistics.Add(new CategoryStats
            {
                CategoryName = category.Name,
                RequestCount = categoryRequests.Count,
                AverageResolutionTime = avgTime
            });
        }

        // Monthly trends (last 12 months)
        var startDate = DateTime.Now.AddMonths(-12);
        var monthlyData = requests
            .Where(r => r.SubmittedAt >= startDate)
            .GroupBy(r => new { r.SubmittedAt.Year, r.SubmittedAt.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyTrend
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                SubmittedCount = g.Count(),
                ResolvedCount = g.Count(r => r.ResolvedAt.HasValue)
            })
            .ToList();

        model.MonthlyTrends = monthlyData;

        // Status distribution
        var statuses = await _context.RequestStatuses.Where(s => s.IsActive).ToListAsync();
        foreach (var status in statuses)
        {
            model.StatusDistribution.Add(new StatusDistribution
            {
                StatusName = status.Name,
                Count = requests.Count(r => r.StatusId == status.StatusId),
                BadgeColor = status.BadgeColor ?? "secondary"
            });
        }

        return View(model);
    }

    // GET: Dashboard/SLA
    public async Task<IActionResult> SLA()
    {
        var requests = await _context.ServiceRequests
            .Include(r => r.Category)
            .Include(r => r.Status)
            .Where(r => r.SLADeadline.HasValue)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        ViewBag.TotalRequests = requests.Count;
        ViewBag.BreachedRequests = requests.Count(r => r.IsSLABreached);
        ViewBag.AchievedRequests = requests.Count(r => !r.IsSLABreached || (r.ResolvedAt.HasValue && r.ResolvedAt <= r.SLADeadline));

        return View(requests);
    }
}

