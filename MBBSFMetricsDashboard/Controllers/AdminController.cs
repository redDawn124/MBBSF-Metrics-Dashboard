using Microsoft.AspNetCore.Mvc;

namespace MBBSFMetricsDashboard.Controllers
{
    public class AdminController : Controller
    {
        // TEMPORARY ADMIN ACCOUNTS
        // These will be replaced with database accounts later.
        private readonly Dictionary<string, string> adminAccounts = new()
        {
            { "taurus", "password123" },
            { "leonard", "password123" },
            { "brenda", "password123" }
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (adminAccounts.ContainsKey(username) &&
                adminAccounts[username] == password)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}