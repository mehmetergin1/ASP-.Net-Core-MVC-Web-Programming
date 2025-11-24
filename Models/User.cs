using System.ComponentModel.DataAnnotations;

namespace CivicRequestPortal.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = "Citizen"; // Citizen, Admin, MunicipalityAdmin

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public virtual ICollection<RequestAssignment> Assignments { get; set; } = new List<RequestAssignment>();
    public virtual ICollection<RequestUpdate> Updates { get; set; } = new List<RequestUpdate>();
}

