using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Controllers;
using MBBSFMetricsDashboard.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Staff/Login";
        options.AccessDeniedPath = "/Staff/AccessDenied";
        options.Cookie.Name = "MBBSF.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;
        options.Events.OnValidatePrincipal = async context =>
        {
            var idValue = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var version = context.Principal?.FindFirst("mbbsf_version")?.Value;
            var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
            var accounts = context.HttpContext.RequestServices.GetRequiredService<AccountService>();
            if (!Guid.TryParse(idValue, out var id) || !accounts.IsSessionValid(id, version, role))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
    var accountId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (action is not null && context.User.Identity?.IsAuthenticated == true &&
        Guid.TryParse(accountId, out var id) &&
        context.RequestServices.GetRequiredService<AccountService>().NeedsPasswordChange(id) &&
        !(action.ControllerName == "Account" && action.ActionName == "ChangePassword") &&
        !(action.ControllerName == "Staff" && action.ActionName == "Logout") &&
        !(action.ControllerName == "Admin" && action.ActionName == "Logout"))
    {
        context.Response.Redirect("/Account/ChangePassword");
        return;
    }
    await next();
});

app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
