using Microsoft.AspNetCore.Identity;

namespace MBBSFMetricsDashboard.Services;

// Iteration 1 only. Replace this service with the foundation's account database.
// Demo accounts are never created outside the Development environment.
public class DemoAccountService
{
    private sealed record Account(string Username, string Role, string PasswordHash);
    private readonly Dictionary<string, Account> accounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly PasswordHasher<string> hasher = new();

    public DemoAccountService(IHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return;
        Add("boardmember", "BoardDemo!2026", "User");

    }

    private void Add(string username, string password, string role)
    {
        accounts[username] = new(username, role, hasher.HashPassword(username, password));
    }

    public string? Validate(string username, string password, string requiredRole)
    {
        if (!accounts.TryGetValue(username.Trim(), out var account) || account.Role != requiredRole)
            return null;

        var result = hasher.VerifyHashedPassword(account.Username, account.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : account.Username;
    }
}
