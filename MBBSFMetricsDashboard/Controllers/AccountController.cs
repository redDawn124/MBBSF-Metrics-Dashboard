using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MBBSFMetricsDashboard.Models;
using MBBSFMetricsDashboard.Services;

namespace MBBSFMetricsDashboard.Controllers;

[Authorize]
public class AccountController(AccountService accounts) : Controller
{
    private Guid CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : Guid.Empty;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Index() => View(accounts.ListAccounts());

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View(new CreateAccountViewModel());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!accounts.CreateAccount(model.FullName, model.Email, model.Role,
                                    model.TemporaryPassword, out var error))
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        TempData["Message"] = "Account created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Edit(Guid id)
    {
        var account = accounts.Find(id);
        if (account is null) return NotFound();
        return View(new EditAccountViewModel
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email,
            Role = account.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid id, EditAccountViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        if (!accounts.UpdateAccount(id, CurrentUserId, model.FullName, model.Email,
                                    model.Role, model.NewPassword, out var error))
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        TempData["Message"] = "Account updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Delete(Guid id)
    {
        var account = accounts.Find(id);
        if (account is null) return NotFound();
        if (account.Role != "User") return BadRequest("Only staff accounts can be deleted.");
        return View(account);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        if (!accounts.DeleteStaff(id, out var error)) return BadRequest(error);
        TempData["Message"] = "Staff account deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel
    {
        MustChangePassword = accounts.NeedsPasswordChange(CurrentUserId)
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        model.MustChangePassword = accounts.NeedsPasswordChange(CurrentUserId);
        if (!ModelState.IsValid) return View(model);
        if (!accounts.ChangeOwnPassword(CurrentUserId, model.CurrentPassword,
                                        model.NewPassword, out var error))
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Message"] = "Password updated. Please sign in again.";
        return RedirectToAction("Index", "Login");
    }
}
