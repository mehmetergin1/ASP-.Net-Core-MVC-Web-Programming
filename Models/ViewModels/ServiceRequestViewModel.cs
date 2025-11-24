using System.ComponentModel.DataAnnotations;

namespace CivicRequestPortal.Models.ViewModels;

public class ServiceRequestViewModel
{
    public int RequestId { get; set; }

    [Required(ErrorMessage = "Başlık gereklidir")]
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama gereklidir")]
    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçiniz")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    // Municipality removed from the application.

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
    [StringLength(200)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(50)]
    [Display(Name = "Telefon")] 
    public string? PhoneNumber { get; set; }

    [Display(Name = "Enlem")]
    public decimal? Latitude { get; set; }

    [Display(Name = "Boylam")]
    public decimal? Longitude { get; set; }

    [Display(Name = "Öncelik")]
    public int Priority { get; set; } = 3;

    // For display
    public string? RequestNumber { get; set; }
    public string? CategoryName { get; set; }
    // Municipality removed from the application.
    public string? StatusName { get; set; }
    public string? StatusBadgeColor { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

