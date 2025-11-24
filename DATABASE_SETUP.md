# Veritabanı Kurulum Rehberi

## Veritabanı İlişkileri

Proje aşağıdaki tablolar ve ilişkileri içermektedir:

### Tablolar ve İlişkiler

1. **Users** (Kullanıcılar)
   - UserId (PK)
   - İlişkiler:
     - ServiceRequests (1-N)
     - RequestAssignments (1-N) - AssignedToUser
     - RequestUpdates (1-N)


3. **Categories** (Kategoriler)
   - CategoryId (PK)
   - MunicipalityId (FK) - Nullable
   - İlişkiler:
   - ServiceRequests (1-N)

4. **RequestStatuses** (Durumlar)
   - StatusId (PK)
   - İlişkiler:
     - ServiceRequests (1-N)

5. **ServiceRequests** (Şikayetler)
   - RequestId (PK)
   - RequestNumber (Unique)
   - UserId (FK)
   - CategoryId (FK)
   - StatusId (FK)
   - İlişkiler:
     - User (N-1)
     - Category (N-1)
     - Status (N-1)
     - RequestAssignments (1-N)
     - RequestUpdates (1-N)

6. **RequestAssignments** (Görev Atamaları)
   - AssignmentId (PK)
   - RequestId (FK)
   - AssignedToUserId (FK)
   - AssignedByUserId (FK) - Nullable
   - İlişkiler:
     - ServiceRequest (N-1)
     - AssignedToUser (N-1)
     - AssignedByUser (N-1) - Nullable

7. **RequestUpdates** (Güncellemeler)
   - UpdateId (PK)
   - RequestId (FK)
   - UserId (FK)
   - İlişkiler:
     - ServiceRequest (N-1)
     - User (N-1)

## Entity Framework Migrations

Eğer mevcut veritabanınızı kullanmak istiyorsanız, migration'ları çalıştırmadan önce tablolarınızın bu yapıya uygun olduğundan emin olun.

### Migration Oluşturma ve Uygulama

```bash
# Migration oluştur
dotnet ef migrations add InitialCreate

# Veritabanına uygula
dotnet ef database update
```

### Connection String

`appsettings.json` dosyasında connection string'i güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CivicRequestPortalDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

## Seed Data

DbContext'te aşağıdaki seed data otomatik olarak eklenir:

- **RequestStatuses**: Submitted, InProgress, Assigned, Resolved, Closed, Rejected

- **User**: Admin user (admin@civicportal.com)
- **Categories**: Road Maintenance, Waste Management, Water & Sewage, Parks & Recreation, Street Lighting

## Önemli Notlar

1. **RequestNumber** alanı unique constraint'e sahiptir
2. Foreign key ilişkilerinde **OnDelete** davranışları:
   - ServiceRequest silindiğinde Assignments ve Updates cascade delete
   - Diğer ilişkilerde Restrict kullanılmıştır
3. SLA takibi için **SLADeadline** ve **IsSLABreached** alanları kullanılır

