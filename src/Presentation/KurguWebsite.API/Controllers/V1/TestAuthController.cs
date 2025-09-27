using Asp.Versioning;
using KurguWebsite.API.Controllers;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class TestAuthController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<TestAuthController> _logger;

    public TestAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<TestAuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet("test-user")]
    public async Task<IActionResult> TestUser()
    {
        var user = await _userManager.FindByEmailAsync("admin@kurguwebsite.com");
        if (user == null)
            return Ok(new { message = "User not found" });

        return Ok(new
        {
            userExists = true,
            email = user.Email,
            emailConfirmed = user.EmailConfirmed,
            isActive = user.IsActive,
            hasPasswordHash = !string.IsNullOrEmpty(user.PasswordHash)
        });
    }

    [HttpPost("test-login")]
    public async Task<IActionResult> TestLogin([FromBody] LoginRequest model)
    {
        try
        {
            // Step 1: Find user
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new { step = "FindUser", success = false, message = "User not found" });

            // Step 2: Check password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordCheck)
                return Ok(new { step = "CheckPassword", success = false, message = "Invalid password" });

            // Step 3: Try SignInManager
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            return Ok(new
            {
                step = "Complete",
                success = signInResult.Succeeded,
                signInResult = new
                {
                    succeeded = signInResult.Succeeded,
                    isLockedOut = signInResult.IsLockedOut,
                    isNotAllowed = signInResult.IsNotAllowed,
                    requiresTwoFactor = signInResult.RequiresTwoFactor
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test login error");
            return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}