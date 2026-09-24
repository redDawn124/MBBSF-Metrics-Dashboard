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

        private static int failedAttempts = 0; //counter for attemps
        private static DateTime? lockoutEndTime = null; //timer for locked out of guessing password

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Check if the admin is currently locked out
            if (lockoutEndTime.HasValue)
            {
                if (DateTime.Now < lockoutEndTime.Value)
                {
                    ViewBag.AccessDenied = true;
                    ViewBag.LockoutEndTime = lockoutEndTime.Value;
                    return View();
                }

                // Lockout has ended, so reset the login attempts
                lockoutEndTime = null;
                failedAttempts = 0;
            }

            // Check the temporary admin username and password
            if (adminAccounts.ContainsKey(username) &&
                adminAccounts[username] == password)
            {
                failedAttempts = 0;
                return RedirectToAction("Dashboard");
            }

            // Incorrect login
            failedAttempts++;

            // Lock the login after 5 failed attempts for 10 minutes
            if (failedAttempts >= 5)
            {
                lockoutEndTime = DateTime.Now.AddMinutes(10);
                ViewBag.AccessDenied = true;
                ViewBag.LockoutEndTime = lockoutEndTime.Value;
            }

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}