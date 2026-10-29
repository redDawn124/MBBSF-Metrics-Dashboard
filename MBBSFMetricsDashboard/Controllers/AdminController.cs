using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MBBSFMetricsDashboard.Models;
using MBBSFMetricsDashboard.Services;

namespace MBBSFMetricsDashboard.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(AccountService accounts) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.IsInRole("Admin")) return RedirectToAction(nameof(Dashboard));
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var account = accounts.Authenticate(model.Username, model.Password, "Admin");
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "The username or password is incorrect.");
            return View(model);
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            AccountService.PrincipalFor(account), new AuthenticationProperties { IsPersistent = false });
        return account.MustChangePassword
            ? RedirectToAction("ChangePassword", "Account")
            : RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Dashboard() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Login");
    }
}
