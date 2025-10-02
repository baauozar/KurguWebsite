// src/Infrastructure/KurguWebsite.Persistence/Seed/SeedingUserService.cs
using KurguWebsite.Application.Common.Interfaces;

namespace KurguWebsite.Persistence.Seed
{
    /// <summary>
    /// A special implementation of ICurrentUserService for database seeding
    /// that always returns system user information
    /// </summary>
    public class SeedingUserService : ICurrentUserService
    {
        public string UserId => "00000000-0000-0000-0000-000000000000";

        public Guid? UserGuidId => Guid.Parse("00000000-0000-0000-0000-000000000000");

        public string UserName => "DatabaseSeeder";

        public string? Email => "seeder@system.local";

        public bool IsAuthenticated => false;

        public bool IsAdmin => true; // Allow seeder to bypass permission checks

        public string IpAddress => "127.0.0.1";

        public bool IsInRole(string role)
        {
            // Seeder has all roles to bypass permission checks during seeding
            return true;
        }
    }
}