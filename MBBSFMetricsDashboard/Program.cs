using Microsoft.AspNetCore.Authentication.Cookies;
using MBBSFMetricsDashboard.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DemoAccountService>();
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
// Keep the existing admin prototype separate, but do not let a signed-in
// board member enter its routes. Admin authentication remains a separate task.
app.Use(async (context, next) =>
{
    if (context.User.IsInRole("User") && context.Request.Path.StartsWithSegments("/Admin"))
    {
        context.Response.Redirect(context.Request.PathBase + "/Staff/AccessDenied");
        return;
    }
    await next();
});
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
