
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MBBSFMetricsDashboard.Controllers;

[Authorize(Roles = "Admin")]
public class UploadController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        IFormFile? CourseraFile,
        IFormFile? GoogleFormsFile,
        IFormFile? CognitoFile)
    {
        var files = new Dictionary<string, IFormFile?>
        {
            { "Coursera", CourseraFile },
            { "Google Forms", GoogleFormsFile },
            { "Cognito", CognitoFile }
        };

        bool fileSelected = files.Values.Any(
            file => file != null && file.Length > 0);

        if (!fileSelected)
        {
            ModelState.AddModelError(
                string.Empty,
                "Please select at least one file.");
        }

        foreach (var item in files)
        {
            string source = item.Key;
            IFormFile? file = item.Value;

            if (file == null || file.Length == 0)
            {
                continue;
            }

            // Cognito accepts PDF. The other sources accept CSV.
            string requiredExtension =
                source == "Cognito" ? ".pdf" : ".csv";

            if (!Path.GetExtension(file.FileName).Equals(
                    requiredExtension,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    source,
                    $"{source}: Please select a {requiredExtension} file.");

                continue;
            }

            // Maximum file size is 5 MB.
            if (file.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    source,
                    $"{source}: The file must be smaller than 5 MB.");

                continue;
            }

            // Temporary validation: read the file without saving it.
            using var reader = new StreamReader(file.OpenReadStream());
            string? firstLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                ModelState.AddModelError(
                    source,
                    $"{source}: The file appears to be empty.");

                continue;
            }

            TempData[$"{source}Upload"] =
                $"{file.FileName} was received. Database import is not enabled yet.";
        }

        return View();
    }
}
