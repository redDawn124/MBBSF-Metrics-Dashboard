using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MBBSFMetricsDashboard.Models;
using MBBSFMetricsDashboard.Services;

namespace MBBSFMetricsDashboard.Controllers;

[Authorize(Roles = "User")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class StaffController(DemoAccountService accounts) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.IsInRole("User")) return RedirectToAction(nameof(Dashboard));
        if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var username = accounts.Validate(model.Username, model.Password, "User");
        if (username is null)
        {
            ModelState.AddModelError(string.Empty, "The username or password is incorrect.");
            return View(model);
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User")
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Dashboard() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
