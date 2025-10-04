// src/Infrastructure/KurguWebsite.Persistence/Seed/DatabaseSeeder.cs
using KurguWebsite.Domain.Constants;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.ValueObjects;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace KurguWebsite.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<KurguWebsiteDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<InvalidProgramException>>();

            try
            {
                logger.LogInformation("Starting database seeding...");

                // Seed Roles FIRST (creates basic roles)
                await SeedRolesAsync(roleManager, logger);
                
                // Seed Users
                await SeedUsersAsync(userManager, logger);
                
                // Seed Roles with Permissions (after users exist)
                await SeedRolesAndPermissionsAsync(roleManager, userManager, logger);
                
                // Seed data entities
                await SeedCompanyInfoAsync(context);
                await SeedServicesAsync(context, logger);
                await SeedPagesAsync(context, logger);
                await SeedTestimonialsAsync(context, logger);
                await SeedPartnersAsync(context, logger);
                await SeedProcessStepsAsync(context, logger);
                await SeedCaseStudiesAsync(context, logger);

                // Save all changes
                await context.SaveChangesAsync();
                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        #region Roles and Permissions

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
        {
            var roles = new[] { "Admin", "User", "Manager", "Editor", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
                    logger.LogInformation("Created role: {Role}", role);
                }
            }
        }

        private static async Task SeedRolesAndPermissionsAsync(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            logger.LogInformation("Seeding permissions...");

            // Admin Role - All permissions
            await CreateRoleWithPermissionsAsync(
                roleManager,
                "Admin",
                Permissions.GetAllPermissions(),
                logger);

            // Editor Role - Content management permissions
            await CreateRoleWithPermissionsAsync(
                roleManager,
                "Editor",
                new[]
                {
                    Permissions.Services.View,
                    Permissions.Services.Create,
                    Permissions.Services.Edit,
                    Permissions.CaseStudies.View,
                    Permissions.CaseStudies.Create,
                    Permissions.CaseStudies.Edit,
                    Permissions.Testimonials.View,
                    Permissions.Testimonials.Create,
                    Permissions.Testimonials.Edit,
                    Permissions.Partners.View,
                    Permissions.Partners.Create,
                    Permissions.Partners.Edit,
                    Permissions.Pages.View,
                    Permissions.Pages.Edit,
                    Permissions.ProcessSteps.View,
                    Permissions.ProcessSteps.Create,
                    Permissions.ProcessSteps.Edit,
                    Permissions.CompanyInfo.View,
                    Permissions.CompanyInfo.Edit,
                    Permissions.ContactMessages.View,
                    Permissions.ContactMessages.Reply,
                    Permissions.Dashboard.View
                },
                logger);

            // Viewer Role - View-only permissions
            var viewPermissions = Permissions.GetAllPermissions()
                .Where(p => p.EndsWith(".View"))
                .ToList();

            await CreateRoleWithPermissionsAsync(
                roleManager,
                "Viewer",
                viewPermissions,
                logger);

            // Manager Role - Most permissions except user management
            var managerPermissions = Permissions.GetAllPermissions()
                .Where(p => !p.StartsWith("Permissions.Users."))
                .ToList();

            await CreateRoleWithPermissionsAsync(
                roleManager,
                "Manager",
                managerPermissions,
                logger);

            // User role - Basic permissions
            await CreateRoleWithPermissionsAsync(
                roleManager,
                "User",
                new[] { Permissions.Dashboard.View },
                logger);

            // Ensure admin user has Admin role
            await EnsureAdminUserAsync(userManager, logger);

            logger.LogInformation("Permissions seeding completed");
        }

        private static async Task CreateRoleWithPermissionsAsync(
            RoleManager<IdentityRole<Guid>> roleManager,
            string roleName,
            IEnumerable<string> permissions,
            ILogger logger)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                role = new IdentityRole<Guid> { Name = roleName };
                await roleManager.CreateAsync(role);
                logger.LogInformation("Created role: {Role}", roleName);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToHashSet();

            var permissionsToAdd = permissions.Where(p => !existingPermissions.Contains(p)).ToList();

            foreach (var permission in permissionsToAdd)
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }

            if (permissionsToAdd.Any())
            {
                logger.LogInformation("Added {Count} permissions to role {Role}", permissionsToAdd.Count, roleName);
            }
        }

        private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@kurguwebsite.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null)
            {
                var roles = await userManager.GetRolesAsync(adminUser);
                if (!roles.Contains("Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Assigned Admin role to {Email}", adminEmail);
                }
            }
        }

        #endregion

        #region Users

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            if (await userManager.Users.AnyAsync())
            {
                logger.LogInformation("Users already exist, skipping user seeding");
                return;
            }

            var users = new[]
            {
                (Email: "admin@kurguwebsite.com", Password: "Admin123!", FirstName: "Admin", LastName: "User", Role: "Admin"),
                (Email: "editor@kurguwebsite.com", Password: "Editor123!", FirstName: "Editor", LastName: "User", Role: "Editor"),
                (Email: "viewer@kurguwebsite.com", Password: "Viewer123!", FirstName: "Viewer", LastName: "User", Role: "Viewer"),
                (Email: "manager@kurguwebsite.com", Password: "Manager123!", FirstName: "Manager", LastName: "User", Role: "Manager"),
                (Email: "user@kurguwebsite.com", Password: "User123!", FirstName: "Normal", LastName: "User", Role: "User")
            };

            foreach (var (email, password, firstName, lastName, role) in users)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    PhoneNumber = $"555-000{Array.IndexOf(users.Select(u => u.Email).ToArray(), email) + 1}"
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation("Created user: {Email} with role {Role}", email, role);
                }
                else
                {
                    logger.LogWarning("Failed to create user {Email}: {Errors}", 
                        email, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        #endregion

        #region Company Info

        private static async Task SeedCompanyInfoAsync(KurguWebsiteDbContext context)
        {
            if (!await context.CompanyInfo.AnyAsync())
            {
                var company = CompanyInfo.Create(
                    "Kurgu IT Services",
                    ContactInfo.Create("800-123-4567", "800-123-4568", "info@kurguwebsite.com"),
                    Address.Create("12345 Porto Blvd", "Suite 1500", "Los Angeles", "California", "90000", "USA"));

                company.UpdateBasicInfo(
                    "Kurgu IT Services",
                    "Leading provider of innovative IT solutions and services.",
                    "To deliver cutting-edge technology solutions that transform businesses.",
                    "To be the most trusted technology partner globally.",
                    "Innovation, Excellence, Partnership");



                await context.CompanyInfo.AddAsync(company);
            }
        }

        #endregion

        #region Services

        private static async Task SeedServicesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Services.AnyAsync())
            {
                logger.LogInformation("Services already exist, skipping");
                return;
            }

            var services = new[]
            {
                Service.Create("Cloud Services", "Scalable cloud solutions", "Transform your infrastructure", "/images/services/cloud.jpg", ServiceCategory.CloudServices),
                Service.Create("Tech Support", "24/7 technical support", "Expert assistance", "/images/services/support.jpg", ServiceCategory.TechSupport),
                Service.Create("Data Security", "Comprehensive security solutions", "Protect your business", "/images/services/security.jpg", ServiceCategory.DataSecurity),
                Service.Create("Software Development", "Custom software solutions", "Build the perfect solution", "/images/services/development.jpg", ServiceCategory.SoftwareDevelopment)
            };

            for (int i = 0; i < services.Length; i++)
            {
                services[i].SetDisplayOrder(i + 1);
                services[i].SetFeatured(i < 2);
            }

            await context.Services.AddRangeAsync(services);
            logger.LogInformation("Seeded {Count} services", services.Length);
        }

        #endregion

        #region Pages

        private static async Task SeedPagesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Pages.AnyAsync())
            {
                logger.LogInformation("Pages already exist, skipping");
                return;
            }

            var pages = new[]
            {
                Page.Create("Home", PageType.Home),
                Page.Create("About Us", PageType.About),
                Page.Create("Services", PageType.Services),
                Page.Create("Contact", PageType.Contact)
            };

            foreach (var page in pages)
            {
                page.SetActive(true);
            }

            await context.Pages.AddRangeAsync(pages);
            logger.LogInformation("Seeded {Count} pages", pages.Length);
        }

        #endregion

        #region Testimonials

        private static async Task SeedTestimonialsAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Testimonials.AnyAsync())
            {
                logger.LogInformation("Testimonials already exist, skipping");
                return;
            }

            var testimonials = new[]
  {
    Testimonial.Create("John Doe", "CEO", "Tech Corp", "Great service!", null, 5),
    Testimonial.Create("Jane Smith", "CTO", "Innovation Inc", "Outstanding support!", null, 5),
    Testimonial.Create("Mike Johnson", "IT Director", "Global Solutions", "Professional and reliable!", null, 5)
};

            for (int i = 0; i < testimonials.Length; i++)
            {
                testimonials[i].SetDisplayOrder(i + 1);
                testimonials[i].SetFeatured(i == 0);
            }

            await context.Testimonials.AddRangeAsync(testimonials);
            logger.LogInformation("Seeded {Count} testimonials", testimonials.Length);
        }

        #endregion

        #region Partners

        private static async Task SeedPartnersAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Partners.AnyAsync())
            {
                logger.LogInformation("Partners already exist, skipping");
                return;
            }

            var partners = new[]
{
    Partner.Create("Microsoft", "/images/partners/microsoft.png", null, null, PartnerType.TechnologyPartner),
    Partner.Create("AWS", "/images/partners/aws.png", null, null, PartnerType.TechnologyPartner),
    Partner.Create("Google Cloud", "/images/partners/google.png", null, null, PartnerType.TechnologyPartner)
};

            for (int i = 0; i < partners.Length; i++)
            {
                partners[i].SetDisplayOrder(i + 1);
            }

            await context.Partners.AddRangeAsync(partners);
            logger.LogInformation("Seeded {Count} partners", partners.Length);
        }

        #endregion

        #region Process Steps

        private static async Task SeedProcessStepsAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.ProcessSteps.AnyAsync())
            {
                logger.LogInformation("Process steps already exist, skipping");
                return;
            }

            var steps = new[]
            {
                ProcessStep.Create(1, "Analysis", "We analyze your requirements"),
                ProcessStep.Create(2, "Design", "We design the perfect solution"),
                ProcessStep.Create(3, "Development", "We build your solution"),
                ProcessStep.Create(4, "Testing", "We ensure quality"),
                ProcessStep.Create(5, "Deployment", "We deploy your solution"),
                ProcessStep.Create(6, "Support", "We provide ongoing support")
            };

            await context.ProcessSteps.AddRangeAsync(steps);
            logger.LogInformation("Seeded {Count} process steps", steps.Length);
        }

        #endregion

        #region Case Studies

        private static async Task SeedCaseStudiesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.CaseStudies.AnyAsync())
            {
                logger.LogInformation("Case studies already exist, skipping");
                return;
            }

            var services = await context.Services.ToListAsync();
            if (!services.Any())
            {
                logger.LogWarning("No services found, skipping case studies");
                return;
            }

            var cases = new[]
            {
                CaseStudy.Create("Digital Transformation for Global Retailer", "Global Retail Corp", "Cloud migration and digital transformation", "/images/cases/retail.jpg", DateTime.UtcNow.AddMonths(-3)),
                CaseStudy.Create("Security Enhancement for Financial Institution", "First National Bank", "Comprehensive security framework implementation", "/images/cases/finance.jpg", DateTime.UtcNow.AddMonths(-6))
            };

            for (int i = 0; i < cases.Length && i < services.Count; i++)
            {
                cases[i].SetService(services[i].Id);
                cases[i].SetFeatured(true);
                cases[i].SetDisplayOrder(i + 1);
                cases[i].AddTechnology("Azure");
                cases[i].AddTechnology("React");
                cases[i].AddTechnology(".NET Core");
            }

            await context.CaseStudies.AddRangeAsync(cases);
            logger.LogInformation("Seeded {Count} case studies", cases.Length);
        }

        #endregion
    }
}