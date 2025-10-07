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

                await context.Database.MigrateAsync();

                await SeedRolesAsync(roleManager, logger);
                await SeedUsersAsync(userManager, logger);
                await SeedRolesAndPermissionsAsync(roleManager, userManager, logger);

                // Seed data entities
                await SeedCompanyInfoAsync(context, logger);
                await SeedServicesAsync(context, logger);
                await SeedPagesAsync(context, logger);
                await SeedTestimonialsAsync(context, logger);
                await SeedPartnersAsync(context, logger);
                await SeedProcessStepsAsync(context, logger);
                await SeedCaseStudiesAsync(context, logger);

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

        private static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger.LogInformation("Seeding permissions...");
            await CreateRoleWithPermissionsAsync(roleManager, "Admin", Permissions.GetAllPermissions(), logger);
            var editorPermissions = new[]
            {
                Permissions.Services.View, Permissions.Services.Create, Permissions.Services.Edit,
                Permissions.CaseStudies.View, Permissions.CaseStudies.Create, Permissions.CaseStudies.Edit,
                Permissions.Testimonials.View, Permissions.Testimonials.Create, Permissions.Testimonials.Edit,
                Permissions.Partners.View, Permissions.Partners.Create, Permissions.Partners.Edit,
                Permissions.Pages.View, Permissions.Pages.Edit,
                Permissions.ProcessSteps.View, Permissions.ProcessSteps.Create, Permissions.ProcessSteps.Edit,
                Permissions.CompanyInfo.View, Permissions.CompanyInfo.Edit,
                Permissions.ContactMessages.View, Permissions.ContactMessages.Reply,
                Permissions.Dashboard.View
            };
            await CreateRoleWithPermissionsAsync(roleManager, "Editor", editorPermissions, logger);
            var viewPermissions = Permissions.GetAllPermissions().Where(p => p.EndsWith(".View")).ToList();
            await CreateRoleWithPermissionsAsync(roleManager, "Viewer", viewPermissions, logger);
            var managerPermissions = Permissions.GetAllPermissions().Where(p => !p.StartsWith("Permissions.Users.")).ToList();
            await CreateRoleWithPermissionsAsync(roleManager, "Manager", managerPermissions, logger);
            await CreateRoleWithPermissionsAsync(roleManager, "User", new[] { Permissions.Dashboard.View }, logger);
            await EnsureAdminUserAsync(userManager, logger);
            logger.LogInformation("Permissions seeding completed");
        }

        private static async Task CreateRoleWithPermissionsAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName, IEnumerable<string> permissions, ILogger logger)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole<Guid> { Name = roleName };
                await roleManager.CreateAsync(role);
                logger.LogInformation("Created role: {Role}", roleName);
            }
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims.Where(c => c.Type == "Permission").Select(c => c.Value).ToHashSet();
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
                ("admin@kurguwebsite.com", "Admin123!", "Admin", "User", "Admin"),
                ("editor@kurguwebsite.com", "Editor123!", "Editor", "User", "Editor"),
                ("viewer@kurguwebsite.com", "Viewer123!", "Viewer", "User", "Viewer"),
                ("manager@kurguwebsite.com", "Manager123!", "Manager", "User", "Manager"),
                ("user@kurguwebsite.com", "User123!", "Normal", "User", "User")
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
                    PhoneNumber = $"555-000{Array.IndexOf(users, Array.Find(users, u => u.Item1 == email)) + 1}"
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation("Created user: {Email} with role {Role}", email, role);
                }
                else
                {
                    logger.LogWarning("Failed to create user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        #endregion

        #region Company Info

        private static async Task SeedCompanyInfoAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.CompanyInfo.AnyAsync()) return;

            var company = CompanyInfo.Create(
                "Kurgu IT Services",
                ContactInfo.Create("800-123-4567", "800-123-4568", "info@kurguwebsite.com"),
                Address.Create("12345 Porto Blvd", "Suite 1500", "Los Angeles", "California", "90000", "USA"),
                yearsInBusiness: 5, totalClients: 100, projectsCompleted: 250, teamMembers: 25
            );

            company.UpdateBasicInfo(
                "Kurgu IT Services",
                "Leading provider of innovative IT solutions and services designed to elevate businesses into the digital future.",
                "To deliver cutting-edge technology solutions that empower our clients to achieve their strategic goals and drive transformation.",
                "To be the most trusted and innovative technology partner for businesses worldwide, recognized for our commitment to excellence and partnership.",
                "Innovation, Excellence, Partnership"
            );

            company.UpdateLogos("/images/logo-dark.png", "/images/logo-light.png");

            company.UpdateSocialMedia(SocialMediaLinks.Create(
                "https://facebook.com", "https://twitter.com", "https://linkedin.com",
                "https://instagram.com", "https://youtube.com"
            ));

            company.UpdateSectionImages("/images/mission.jpg", "/images/vision.jpg");

            await context.CompanyInfo.AddAsync(company);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded company information");
        }

        #endregion

        #region Data Entities

        private static async Task SeedServicesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Services.AnyAsync()) return;

            var services = new[]
            {
        Service.Create("Cloud Services", "End-to-end cloud solutions...", "Scalable cloud solutions", "/images/services/cloud.jpg", ServiceCategory.CloudServices, "bi-cloud"),
        Service.Create("Tech Support", "Reliable, 24/7 technical support...", "24/7 technical support", "/images/services/support.jpg", ServiceCategory.TechSupport, "bi-headset"),
        Service.Create("Data Security", "Comprehensive security to protect your data...", "Comprehensive security solutions", "/images/services/security.jpg", ServiceCategory.DataSecurity, "bi-shield-lock"),
        Service.Create("Software Development", "Custom software development...", "Custom software solutions", "/images/services/development.jpg", ServiceCategory.SoftwareDevelopment, "bi-code-slash")
    };

            for (int i = 0; i < services.Length; i++)
            {
                // ✅ CORRECTED: Start display order from 1
                services[i].SetDisplayOrder(i + 1);
                services[i].SetFeatured(i < 2);
            }

            await context.Services.AddRangeAsync(services);
            logger.LogInformation("Seeded {Count} services", services.Length);
        }
        private static async Task SeedPagesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Pages.AnyAsync()) return;

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
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} pages", pages.Length);
        }

        private static async Task SeedTestimonialsAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Testimonials.AnyAsync()) return;

            var testimonials = new[]
            {
                Testimonial.Create("John Doe", "CEO", "Tech Corp", "Kurgu's cloud services transformed our infrastructure, boosting efficiency by 40%. An outstanding partner!", "/images/testimonials/user1.jpg", 5),
                Testimonial.Create("Jane Smith", "CTO", "Innovation Inc", "The 24/7 tech support is phenomenal. Their team is knowledgeable, fast, and incredibly reliable.", "/images/testimonials/user2.jpg", 5),
                Testimonial.Create("Mike Johnson", "IT Director", "Global Solutions", "Their data security solutions are top-notch. We feel more secure than ever.", "/images/testimonials/user3.jpg", 5)
            };

            for (int i = 0; i < testimonials.Length; i++)
            {
                testimonials[i].SetDisplayOrder(i+1);
                testimonials[i].SetFeatured(i == 0);
            }

            await context.Testimonials.AddRangeAsync(testimonials);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} testimonials", testimonials.Length);
        }

        private static async Task SeedPartnersAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.Partners.AnyAsync()) return;

            var partners = new[]
            {
                Partner.Create("Microsoft", "/images/partners/microsoft.png", "https://microsoft.com", "A leading technology partner.", PartnerType.TechnologyPartner),
                Partner.Create("AWS", "/images/partners/aws.png", "https://aws.amazon.com", "A key cloud infrastructure partner.", PartnerType.TechnologyPartner),
                Partner.Create("Google Cloud", "/images/partners/google.png", "https://cloud.google.com", "A strategic cloud partner.", PartnerType.TechnologyPartner)
            };

            for (int i = 0; i < partners.Length; i++)
            {
                partners[i].SetDisplayOrder(i+1);
            }

            await context.Partners.AddRangeAsync(partners);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} partners", partners.Length);
        }

        private static async Task SeedProcessStepsAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.ProcessSteps.AnyAsync())
            {
                logger.LogInformation("Process steps already exist, skipping");
                return;
            }

            var steps = new[]
            {
        // ✅ CORRECTED: Pass the StepNumber as the first argument
        ProcessStep.Create( "Analysis", "We meticulously analyze your requirements to understand your business goals.", "bi-search"),
        ProcessStep.Create( "Design", "We design a tailored solution and architecture that aligns with your strategic vision.", "bi-pencil-square"),
        ProcessStep.Create( "Development", "Our expert developers build your solution using the latest technologies.", "bi-code-slash",3),
        ProcessStep.Create( "Testing", "We conduct rigorous testing to ensure the highest standards of quality and performance.", "bi-check2-circle"),
        ProcessStep.Create( "Deployment", "We seamlessly deploy your solution into your production environment.", "bi-rocket-launch"),
        ProcessStep.Create( "Support", "Our team provides ongoing support and maintenance to ensure long-term success.", "bi-headset")
    };

            // ✅ ADDED: Loop to set the display order for each step
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].SetDisplayOrder(i + 1);
            }

            await context.ProcessSteps.AddRangeAsync(steps);
            // ✅ REMOVED: SaveChangesAsync() will be called once at the end in the main SeedAsync method.
            logger.LogInformation("Seeded {Count} process steps", steps.Length);
        }
        private static async Task SeedCaseStudiesAsync(KurguWebsiteDbContext context, ILogger logger)
        {
            if (await context.CaseStudies.AnyAsync()) return;
            var services = await context.Services.ToListAsync();
            if (!services.Any())
            {
                logger.LogWarning("No services found, skipping case studies seeding.");
                return;
            }

            var cases = new[]
            {
                CaseStudy.Create("Digital Transformation for Global Retailer", "Global Retail Corp", "A complete cloud migration and digital platform overhaul.", "/images/cases/retail.jpg", DateTime.UtcNow.AddMonths(-3)),
                CaseStudy.Create("Security Enhancement for Financial Institution", "First National Bank", "Implementation of a comprehensive security framework.", "/images/cases/finance.jpg", DateTime.UtcNow.AddMonths(-6))
            };

            for (int i = 0; i < cases.Length; i++)
            {
                cases[i].SetService(services[i % services.Count].Id);
                cases[i].SetFeatured(true);
                cases[i].SetDisplayOrder(i+1);
                cases[i].AddTechnology("Azure");
                cases[i].AddTechnology("React");
                cases[i].AddTechnology(".NET Core");
            }

            await context.CaseStudies.AddRangeAsync(cases);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} case studies", cases.Length);
        }

        #endregion
    }
}