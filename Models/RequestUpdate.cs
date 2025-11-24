using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicRequestPortal.Models;

public class RequestUpdate
{
    [Key]
    public int UpdateId { get; set; }

    [Required]
    public int RequestId { get; set; }

    [ForeignKey("RequestId")]
    public virtual ServiceRequest Request { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsInternal { get; set; } = false; // Internal notes not visible to citizens

    [StringLength(50)]
    public string? UpdateType { get; set; } // StatusChange, Comment, Assignment, etc.
}

