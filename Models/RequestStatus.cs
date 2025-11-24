using System.ComponentModel.DataAnnotations;

namespace CivicRequestPortal.Models;

public class RequestStatus
{
    [Key]
    public int StatusId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // Submitted, InProgress, Assigned, Resolved, Closed, Rejected

    [StringLength(200)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? BadgeColor { get; set; } // Bootstrap badge color (primary, success, warning, etc.)

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}

