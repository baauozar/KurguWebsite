using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.ValueObjects;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            // Ensure DB exists and migrate
            await context.Database.MigrateAsync();

            // Seed Roles and Users
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);

            // Seed data entities
            await SeedCompanyInfoAsync(context);
            await SeedServicesAsync(context);
            await SeedPagesAsync(context);
            await SeedTestimonialsAsync(context);
            await SeedPartnersAsync(context);
            await SeedProcessStepsAsync(context);
            await SeedCaseStudiesAsync(context);

            // Save all changes
            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] { "Admin", "User", "Manager" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            if (!await userManager.Users.AnyAsync())
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@kurguwebsite.com",
                    Email = "admin@kurguwebsite.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                if ((await userManager.CreateAsync(admin, "Admin123!")).Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");

                var user = new ApplicationUser
                {
                    UserName = "user@kurguwebsite.com",
                    Email = "user@kurguwebsite.com",
                    FirstName = "Normal",
                    LastName = "User",
                    EmailConfirmed = true
                };

                if ((await userManager.CreateAsync(user, "User123!")).Succeeded)
                    await userManager.AddToRoleAsync(user, "User");
            }
        }

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

                company.SetCreatedBy("Seeder");

                await context.CompanyInfo.AddAsync(company);
            }
        }

        private static async Task SeedServicesAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Services.AnyAsync())
            {
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
                    services[i].SetCreatedBy("Seeder");
                }

                await context.Services.AddRangeAsync(services);
            }
        }

        private static async Task SeedPagesAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Pages.AnyAsync())
            {
                var pages = new[]
                {
                    Page.Create("Home", PageType.Home),
                    Page.Create("About Us", PageType.About),
                    Page.Create("Services", PageType.Services),
                    Page.Create("Contact", PageType.Contact)
                };

                foreach (var page in pages)
                    page.SetCreatedBy("Seeder");

                await context.Pages.AddRangeAsync(pages);
            }
        }

        private static async Task SeedTestimonialsAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Testimonials.AnyAsync())
            {
                var testimonials = new[]
                {
                    Testimonial.Create("John Doe", "CEO", "Tech Corp", "Great service!", 5),
                    Testimonial.Create("Jane Smith", "CTO", "Innovation Inc", "Outstanding support!", 5),
                    Testimonial.Create("Mike Johnson", "IT Director", "Global Solutions", "Professional and reliable!", 5)
                };

                foreach (var t in testimonials)
                {
                    t.SetDisplayOrder(1);
                    t.SetFeatured(true);
                    t.SetCreatedBy("Seeder");
                }

                await context.Testimonials.AddRangeAsync(testimonials);
            }
        }

        private static async Task SeedPartnersAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Partners.AnyAsync())
            {
                var partners = new[]
                {
                    Partner.Create("Microsoft", "/images/partners/microsoft.png", PartnerType.TechnologyPartner),
                    Partner.Create("AWS", "/images/partners/aws.png", PartnerType.TechnologyPartner),
                    Partner.Create("Google Cloud", "/images/partners/google.png", PartnerType.TechnologyPartner)
                };

                foreach (var p in partners)
                    p.SetCreatedBy("Seeder");

                await context.Partners.AddRangeAsync(partners);
            }
        }

        private static async Task SeedProcessStepsAsync(KurguWebsiteDbContext context)
        {
            if (!await context.ProcessSteps.AnyAsync())
            {
                var steps = new[]
                {
                    ProcessStep.Create(1, "Analysis", "We analyze your requirements"),
                    ProcessStep.Create(2, "Design", "We design the perfect solution"),
                    ProcessStep.Create(3, "Development", "We build your solution"),
                    ProcessStep.Create(4, "Testing", "We ensure quality"),
                    ProcessStep.Create(5, "Deployment", "We deploy your solution"),
                    ProcessStep.Create(6, "Support", "We provide ongoing support")
                };

                foreach (var step in steps)
                    step.SetCreatedBy("Seeder");

                await context.ProcessSteps.AddRangeAsync(steps);
            }
        }

        private static async Task SeedCaseStudiesAsync(KurguWebsiteDbContext context)
        {
            if (!await context.CaseStudies.AnyAsync())
            {
                var services = await context.Services.ToListAsync();
                if (!services.Any()) return;

                var cases = new[]
                {
                    CaseStudy.Create("Digital Transformation for Global Retailer", "Global Retail Corp", "Cloud migration", "/images/cases/retail.jpg", DateTime.UtcNow.AddMonths(-3)),
                    CaseStudy.Create("Security Enhancement for Financial Institution", "First National Bank", "Security framework", "/images/cases/finance.jpg", DateTime.UtcNow.AddMonths(-6))
                };

                for (int i = 0; i < cases.Length && i < services.Count; i++)
                {
                    cases[i].SetService(services[i].Id);
                    cases[i].SetFeatured(true);
                    cases[i].SetCreatedBy("Seeder");
                }

                await context.CaseStudies.AddRangeAsync(cases);
            }
        }
    }
}
