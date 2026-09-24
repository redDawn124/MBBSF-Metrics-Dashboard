using System.ComponentModel.DataAnnotations;

namespace MBBSFMetricsDashboard.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
