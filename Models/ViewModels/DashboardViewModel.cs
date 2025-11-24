namespace CivicRequestPortal.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int ResolvedRequests { get; set; }
    public int ClosedRequests { get; set; }
    public double AverageResolutionTime { get; set; } // in hours
    public double SLAAchievementRate { get; set; } // percentage
    public int SLABreachedRequests { get; set; }
    public List<CategoryStats> CategoryStatistics { get; set; } = new();
    public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
    public List<StatusDistribution> StatusDistribution { get; set; } = new();
}

public class CategoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public double AverageResolutionTime { get; set; }
}

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public int SubmittedCount { get; set; }
    public int ResolvedCount { get; set; }
}

public class StatusDistribution
{
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public string BadgeColor { get; set; } = string.Empty;
}

