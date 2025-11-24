using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicRequestPortal.Models;

public class ServiceRequest
{
    [Key]
    public int RequestId { get; set; }

    [Required]
    [StringLength(50)]
    public string RequestNumber { get; set; } = string.Empty; // Unique request number

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;

    [Required]
    public int StatusId { get; set; }

    [ForeignKey("StatusId")]
    public virtual RequestStatus Status { get; set; } = null!;

    // Location fields for map
    [StringLength(200)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(10, 8)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(11, 8)")]
    public decimal? Longitude { get; set; }

    // SLA tracking
    public DateTime SubmittedAt { get; set; } = DateTime.Now;

    public DateTime? AssignedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? SLAHours { get; set; } // SLA in hours for this request

    public DateTime? SLADeadline { get; set; }

    public bool IsSLABreached { get; set; } = false;

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    [StringLength(500)]
    public string? Attachments { get; set; } // Comma-separated file paths

    public int Priority { get; set; } = 3; // 1=High, 2=Medium, 3=Low

    // Navigation properties
    public virtual ICollection<RequestAssignment> Assignments { get; set; } = new List<RequestAssignment>();
    public virtual ICollection<RequestUpdate> Updates { get; set; } = new List<RequestUpdate>();
}

