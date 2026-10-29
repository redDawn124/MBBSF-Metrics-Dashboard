using System.ComponentModel.DataAnnotations;

namespace MBBSFMetricsDashboard.Models;

public class CreateAccountViewModel
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [Required, RegularExpression("^(Admin|User)$")]
    public string Role { get; set; } = "User";

    [Required, MinLength(10), DataType(DataType.Password)]
    public string TemporaryPassword { get; set; } = "";
}

public class EditAccountViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [Required, RegularExpression("^(Admin|User)$")]
    public string Role { get; set; } = "User";

    [MinLength(10), DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string? ConfirmNewPassword { get; set; }
}

public class ChangePasswordViewModel
{
    public bool MustChangePassword { get; set; }

    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(10), DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match."), DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = "";
}
