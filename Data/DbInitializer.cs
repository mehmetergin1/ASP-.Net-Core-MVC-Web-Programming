using System;
using Microsoft.AspNetCore.Builder;

namespace CivicRequestPortal.Data;

// Minimal no-op initializer to satisfy startup references.
public static class DbInitializer
{
    // Called as DbInitializer.Initialize(app) in some setups
    public static void Initialize(WebApplication app)
    {
        // Intentionally left blank. If you need DB seeding here,
        // implement logic to create scope and seed data.
    }

    // Alternative overload accepting a service provider
    public static void Initialize(IServiceProvider serviceProvider)
    {
        // Intentionally left blank.
    }

    // Overload that accepts the application's DbContext directly
    public static void Initialize(ApplicationDbContext context)
    {
        // Intentionally left blank. Add seeding logic here if needed.
    }
}
