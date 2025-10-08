using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

// DI extensions
using KurguWebsite.Application.Features.DependencyInjection; // AddApplication()
using KurguWebsite.Persistence;                              // AddPersistence()
using KurguWebsite.Infrastructure;                           // AddInfrastructure()

var builder = WebApplication.CreateBuilder(args);

// 1) Uygulama katmanları
builder.Services.AddApplication();                          // MediatR + Validators + Automapper + Behaviors
builder.Services.AddPersistence(builder.Configuration);     // DbContext + Repos
builder.Services.AddInfrastructureWeb(builder.Configuration);  // Identity + CurrentUserService + JWT/Caching/Email...

// 2) UI (MVC)
var mvc = builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(KurguWebsite.WebUI.Mappings.WebUiMappingProfile).Assembly);

// 3) UI tarafında cookie’yi varsayılan yap (Infrastructure’da JWT varsa onu override et)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

var app = builder.Build();

// 4) Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// wwwroot/uploads ⇒ /uploads yayını
var uploadPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads");
if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseRouting();

// CORS / RateLimiter tanımlıysa kullanmak istersen aç:
// app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");
// app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Areas + Default routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
