using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;
using KurguWebsite.Domain.ValueObjects;
using KurguWebsite.Infrastructure.Identity;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedCompanyInfoAsync(context);
            await SeedServicesAsync(context);
            await SeedPagesAsync(context);
            await SeedTestimonialsAsync(context);
            await SeedPartnersAsync(context);
            await SeedProcessStepsAsync(context);
            await SeedCaseStudiesAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User", "Manager" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            if (!await userManager.Users.AnyAsync())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@kurguwebsite.com",
                    Email = "admin@kurguwebsite.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                var normalUser = new ApplicationUser
                {
                    UserName = "user@kurguwebsite.com",
                    Email = "user@kurguwebsite.com",
                    FirstName = "Normal",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(normalUser, "User123!");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }

        private static async Task SeedCompanyInfoAsync(KurguWebsiteDbContext context)
        {
            if (!await context.CompanyInfo.AnyAsync())
            {
                var contactInfo = ContactInfo.Create(
                    "800-123-4567",
                    "800-123-4568",
                    "info@kurguwebsite.com");

                var address = Address.Create(
                    "12345 Porto Blvd",
                    "Suite 1500",
                    "Los Angeles",
                    "California",
                    "90000",
                    "USA");

                var companyInfo = CompanyInfo.Create("Kurgu IT Services", contactInfo, address);
                companyInfo.UpdateBasicInfo(
                    "Kurgu IT Services",
                    "Leading provider of innovative IT solutions and services.",
                    "To deliver cutting-edge technology solutions that transform businesses.",
                    "To be the most trusted technology partner globally.",
                    "Innovation, Excellence, Partnership");

                companyInfo.SetCreatedBy("Seeder");

                await context.CompanyInfo.AddAsync(companyInfo);
            }
        }

        private static async Task SeedServicesAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Services.AnyAsync())
            {
                var services = new[]
                {
                    Service.Create(
                        "Cloud Services",
                        "Scalable cloud solutions for modern businesses",
                        "Transform your infrastructure with cloud technology",
                        "/images/services/cloud.jpg",
                        ServiceCategory.CloudServices),

                    Service.Create(
                        "Tech Support",
                        "24/7 technical support for your IT needs",
                        "Expert assistance whenever you need it",
                        "/images/services/support.jpg",
                        ServiceCategory.TechSupport),

                    Service.Create(
                        "Data Security",
                        "Comprehensive security solutions for your data",
                        "Protect your business from cyber threats",
                        "/images/services/security.jpg",
                        ServiceCategory.DataSecurity),

                    Service.Create(
                        "Software Development",
                        "Custom software solutions tailored to your needs",
                        "Build the perfect solution for your business",
                        "/images/services/development.jpg",
                        ServiceCategory.SoftwareDevelopment)
                };

                for (int i = 0; i < services.Length; i++)
                {
                    services[i].SetDisplayOrder(i + 1);
                    services[i].SetFeatured(i < 2);
                    services[i].SetCreatedBy("Seeder");

                    // Add features to each service
                    var feature1 = ServiceFeature.Create(
                        services[i].Id,
                        $"Feature 1 for {services[i].Title}",
                        "Comprehensive feature description",
                        "fas fa-check");

                    var feature2 = ServiceFeature.Create(
                        services[i].Id,
                        $"Feature 2 for {services[i].Title}",
                        "Advanced capabilities and benefits",
                        "fas fa-star");

                    services[i].AddFeature(feature1);
                    services[i].AddFeature(feature2);
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
                    CreatePage(PageType.Home, "Home", "Welcome to Kurgu IT Services"),
                    CreatePage(PageType.About, "About Us", "Learn more about our company"),
                    CreatePage(PageType.Services, "Services", "Our comprehensive IT services"),
                    CreatePage(PageType.Contact, "Contact", "Get in touch with us")
                };

                foreach (var page in pages)
                {
                    page.SetCreatedBy("Seeder");
                }

                await context.Pages.AddRangeAsync(pages);
            }
        }

        private static Page CreatePage(PageType pageType, string title, string description)
        {
            var page = Page.Create(title, pageType);
            page.UpdateHeroSection(
                title.ToUpper(),
                null,
                description,
                null,
                null,
                null);
            return page;
        }

        private static async Task SeedTestimonialsAsync(KurguWebsiteDbContext context)
        {
            if (!await context.Testimonials.AnyAsync())
            {
                var testimonials = new[]
                {
                    Testimonial.Create(
                        "John Doe",
                        "CEO",
                        "Tech Corp",
                        "Kurgu IT Services has transformed our business with their innovative solutions. Highly recommended!",
                        5),

                    Testimonial.Create(
                        "Jane Smith",
                        "CTO",
                        "Innovation Inc",
                        "Outstanding support and expertise. They've been instrumental in our digital transformation.",
                        5),

                    Testimonial.Create(
                        "Mike Johnson",
                        "Director of IT",
                        "Global Solutions",
                        "Professional, reliable, and always delivering on time. A true technology partner.",
                        5)
                };

                for (int i = 0; i < testimonials.Length; i++)
                {
                    testimonials[i].SetDisplayOrder(i + 1);
                    testimonials[i].SetFeatured(i == 0);
                    testimonials[i].SetCreatedBy("Seeder");
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
                    Partner.Create(
                        "Microsoft",
                        "/images/partners/microsoft.png",
                        PartnerType.TechnologyPartner),

                    Partner.Create(
                        "Amazon Web Services",
                        "/images/partners/aws.png",
                        PartnerType.TechnologyPartner),

                    Partner.Create(
                        "Google Cloud",
                        "/images/partners/google.png",
                        PartnerType.TechnologyPartner)
                };

                for (int i = 0; i < partners.Length; i++)
                {
                    partners[i].SetDisplayOrder(i + 1);
                    partners[i].SetCreatedBy("Seeder");
                }

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
                {
                    step.SetCreatedBy("Seeder");
                }

                await context.ProcessSteps.AddRangeAsync(steps);
            }
        }

        private static async Task SeedCaseStudiesAsync(KurguWebsiteDbContext context)
        {
            if (!await context.CaseStudies.AnyAsync())
            {
                var services = await context.Services.ToListAsync();

                if (services.Any())
                {
                    var caseStudies = new[]
                    {
                        CaseStudy.Create(
                            "Digital Transformation for Global Retailer",
                            "Global Retail Corp",
                            "Complete cloud migration and modernization of legacy systems",
                            "/images/cases/retail.jpg",
                            DateTime.Now.AddMonths(-3)),

                        CaseStudy.Create(
                            "Security Enhancement for Financial Institution",
                            "First National Bank",
                            "Implementation of comprehensive security framework",
                            "/images/cases/finance.jpg",
                            DateTime.Now.AddMonths(-6))
                    };

                    for (int i = 0; i < caseStudies.Length && i < services.Count; i++)
                    {
                        caseStudies[i].SetService(services[i].Id);
                        caseStudies[i].SetFeatured(true);
                        caseStudies[i].AddTechnology("Azure");
                        caseStudies[i].AddTechnology(".NET Core");
                        caseStudies[i].AddTechnology("React");
                        caseStudies[i].SetCreatedBy("Seeder");
                    }

                    await context.CaseStudies.AddRangeAsync(caseStudies);
                }
            }
        }
    }
}