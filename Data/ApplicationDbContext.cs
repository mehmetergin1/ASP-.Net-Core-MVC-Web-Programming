using Microsoft.EntityFrameworkCore;
using CivicRequestPortal.Models;

namespace CivicRequestPortal.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RequestStatus> RequestStatuses { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<RequestAssignment> RequestAssignments { get; set; }
    public DbSet<RequestUpdate> RequestUpdates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and constraints

        // User - ServiceRequest (One-to-Many)
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.User)
            .WithMany(u => u.ServiceRequests)
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category - ServiceRequest (One-to-Many)
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Category)
            .WithMany(c => c.ServiceRequests)
            .HasForeignKey(sr => sr.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Status - ServiceRequest (One-to-Many)
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Status)
            .WithMany(s => s.ServiceRequests)
            .HasForeignKey(sr => sr.StatusId)
            .OnDelete(DeleteBehavior.Restrict);


        // ServiceRequest - RequestAssignment (One-to-Many)
        modelBuilder.Entity<RequestAssignment>()
            .HasOne(ra => ra.Request)
            .WithMany(sr => sr.Assignments)
            .HasForeignKey(ra => ra.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - RequestAssignment (AssignedTo) (One-to-Many)
        modelBuilder.Entity<RequestAssignment>()
            .HasOne(ra => ra.AssignedToUser)
            .WithMany(u => u.Assignments)
            .HasForeignKey(ra => ra.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ServiceRequest - RequestUpdate (One-to-Many)
        modelBuilder.Entity<RequestUpdate>()
            .HasOne(ru => ru.Request)
            .WithMany(sr => sr.Updates)
            .HasForeignKey(ru => ru.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - RequestUpdate (One-to-Many)
        modelBuilder.Entity<RequestUpdate>()
            .HasOne(ru => ru.User)
            .WithMany(u => u.Updates)
            .HasForeignKey(ru => ru.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on RequestNumber
        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(sr => sr.RequestNumber)
            .IsUnique();

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed RequestStatuses (Türkçe)
        modelBuilder.Entity<RequestStatus>().HasData(
            new RequestStatus { StatusId = 1, Name = "Gönderildi", Description = "Şikayet gönderildi", BadgeColor = "secondary", DisplayOrder = 1 },
            new RequestStatus { StatusId = 2, Name = "İnceleniyor", Description = "Şikayet inceleniyor", BadgeColor = "primary", DisplayOrder = 2 },
            new RequestStatus { StatusId = 3, Name = "Atandı", Description = "Şikayet personele atandı", BadgeColor = "info", DisplayOrder = 3 },
            new RequestStatus { StatusId = 4, Name = "Çözüldü", Description = "Şikayet çözüldü", BadgeColor = "success", DisplayOrder = 4 },
            new RequestStatus { StatusId = 5, Name = "Kapandı", Description = "Şikayet kapatıldı", BadgeColor = "dark", DisplayOrder = 5 },
            new RequestStatus { StatusId = 6, Name = "Reddedildi", Description = "Şikayet reddedildi", BadgeColor = "danger", DisplayOrder = 6 }
        );


        // Seed default admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                FirstName = "Admin",
                LastName = "Kullanıcı",
                Email = "admin@civicportal.com",
                UserType = "Admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            }
        );

        // Seed default categories (Türkçe)
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Yol Bakım ve Onarım", Description = "Çukurlar, yol onarımları, işaretler", DefaultSLAHours = 72, CreatedAt = DateTime.Now, IsActive = true },
            new Category { CategoryId = 2, Name = "Atık Yönetimi", Description = "Çöp toplama, geri dönüşüm", DefaultSLAHours = 48, CreatedAt = DateTime.Now, IsActive = true },
            new Category { CategoryId = 3, Name = "Su ve Kanalizasyon", Description = "Su sızıntıları, kanalizasyon sorunları", DefaultSLAHours = 24, CreatedAt = DateTime.Now, IsActive = true },
            new Category { CategoryId = 4, Name = "Parklar ve Rekreasyon", Description = "Park bakımı, oyun alanları", DefaultSLAHours = 96, CreatedAt = DateTime.Now, IsActive = true },
            new Category { CategoryId = 5, Name = "Sokak Aydınlatması", Description = "Kırık sokak lambaları", DefaultSLAHours = 48,  CreatedAt = DateTime.Now, IsActive = true }
        );
    }
}

