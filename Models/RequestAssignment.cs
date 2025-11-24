using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicRequestPortal.Models;

public class RequestAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    [Required]
    public int RequestId { get; set; }

    [ForeignKey("RequestId")]
    public virtual ServiceRequest Request { get; set; } = null!;

    [Required]
    public int AssignedToUserId { get; set; }

    [ForeignKey("AssignedToUserId")]
    public virtual User AssignedToUser { get; set; } = null!;

    public int? AssignedByUserId { get; set; }

    [ForeignKey("AssignedByUserId")]
    public virtual User? AssignedByUser { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.Now;

    public DateTime? CompletedAt { get; set; }

    public bool IsActive { get; set; } = true;
}

