using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace MBBSFMetricsDashboard.Services;

public sealed record AccountInfo(
    Guid Id, string Username, string FullName, string Email,
    string Role, bool MustChangePassword, string SessionVersion);

public sealed class StoredAccount
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User";
    public string PasswordHash { get; set; } = "";
    public bool MustChangePassword { get; set; }
    public string SessionVersion { get; set; } = "";
}

public sealed class AccountService
{
    private readonly object gate = new();
    private readonly PasswordHasher<StoredAccount> hasher = new();
    private readonly string filePath;
    private readonly List<StoredAccount> accounts;
    private readonly Dictionary<string, (int Failures, DateTimeOffset LockedUntil)> failedLogins = new();

    public AccountService(IHostEnvironment environment)
    {
        filePath = Path.Combine(environment.ContentRootPath, "App_Data", "accounts.json");
        accounts = File.Exists(filePath)
            ? JsonSerializer.Deserialize<List<StoredAccount>>(File.ReadAllText(filePath)) ?? []
            : [];

        if (environment.IsDevelopment() && accounts.Count == 0)
        {
            AddSeed("boardmember", "Board Member", "boardmember@mbbsf.local", "User", "BoardDemo!2026");
            AddSeed("taurus", "Taurus", "taurus@mbbsf.local", "Admin", "password123");
            AddSeed("leonard", "Leonard", "leonard@mbbsf.local", "Admin", "password123");
            AddSeed("brenda", "Brenda", "brenda@mbbsf.local", "Admin", "password123");
            Save();
        }
    }

    private static AccountInfo Info(StoredAccount a) =>
        new(a.Id, a.Username, a.FullName, a.Email, a.Role, a.MustChangePassword, a.SessionVersion);

    private void AddSeed(string username, string name, string email, string role, string password)
    {
        var account = new StoredAccount
        {
            Id = Guid.NewGuid(), Username = username, FullName = name,
            Email = email, Role = role, SessionVersion = Guid.NewGuid().ToString("N")
        };
        account.PasswordHash = hasher.HashPassword(account, password);
        accounts.Add(account);
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
        var temporaryFile = filePath + ".tmp";
        File.WriteAllText(temporaryFile, JsonSerializer.Serialize(accounts));
        File.Move(temporaryFile, filePath, true);
    }

    public IReadOnlyList<AccountInfo> ListAccounts()
    {
        lock (gate)
            return accounts.OrderBy(a => a.FullName).Select(Info).ToList();
    }

    public AccountInfo? Find(Guid id)
    {
        lock (gate)
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);
            return account is null ? null : Info(account);
        }
    }

    public AccountInfo? Authenticate(string login, string password, string requiredRole)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password)) return null;
        var normalizedLogin = login.Trim();
        var key = normalizedLogin.ToLowerInvariant();
        lock (gate)
        {
            if (failedLogins.TryGetValue(key, out var previous) &&
                previous.LockedUntil > DateTimeOffset.UtcNow)
                return null;

            var account = accounts.FirstOrDefault(a =>
                (string.Equals(a.Username, normalizedLogin, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(a.Email, normalizedLogin, StringComparison.OrdinalIgnoreCase)) &&
                a.Role == requiredRole);
            var result = account is null ? PasswordVerificationResult.Failed
                : hasher.VerifyHashedPassword(account, account.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                var failures = failedLogins.TryGetValue(key, out previous) ? previous.Failures + 1 : 1;
                failedLogins[key] = (failures >= 5 ? 0 : failures,
                    failures >= 5 ? DateTimeOffset.UtcNow.AddMinutes(10) : DateTimeOffset.MinValue);
                return null;
            }
            failedLogins.Remove(key);
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account!.PasswordHash = hasher.HashPassword(account, password);
                Save();
            }
            return Info(account!);
        }
    }

    public bool IsSessionValid(Guid id, string? version, string? role)
    {
        lock (gate)
            return accounts.Any(a => a.Id == id && a.SessionVersion == version && a.Role == role);
    }

    public bool NeedsPasswordChange(Guid id)
    {
        lock (gate)
            return accounts.Any(a => a.Id == id && a.MustChangePassword);
    }

    public static ClaimsPrincipal PrincipalFor(AccountInfo account)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role),
            new Claim("mbbsf_version", account.SessionVersion)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public bool CreateAccount(string fullName, string email, string role, string password, out string error)
    {
        error = "";
        email = email.Trim();
        fullName = fullName.Trim();
        if (role is not ("Admin" or "User") || password.Length < 10)
        {
            error = "Select a valid role and use a password of at least 10 characters.";
            return false;
        }
        lock (gate)
        {
            if (accounts.Any(a => string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(a.Username, email, StringComparison.OrdinalIgnoreCase)))
            {
                error = "An account with this email already exists.";
                return false;
            }
            var account = new StoredAccount
            {
                Id = Guid.NewGuid(), Username = email, FullName = fullName,
                Email = email, Role = role, MustChangePassword = true,
                SessionVersion = Guid.NewGuid().ToString("N")
            };
            account.PasswordHash = hasher.HashPassword(account, password);
            accounts.Add(account);
            Save();
            return true;
        }
    }

    public bool UpdateAccount(Guid id, Guid actingAdminId, string fullName, string email,
        string role, string? newPassword, out string error)
    {
        error = "";
        fullName = fullName.Trim();
        email = email.Trim();
        if (role is not ("Admin" or "User"))
        {
            error = "Invalid account role.";
            return false;
        }
        lock (gate)
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);
            if (account is null) { error = "Account not found."; return false; }
            if (id == actingAdminId && role != "Admin")
            {
                error = "You cannot remove your own admin access.";
                return false;
            }
            if (accounts.Any(a => a.Id != id &&
                (string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(a.Username, email, StringComparison.OrdinalIgnoreCase))))
            {
                error = "An account with this email already exists.";
                return false;
            }
            if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 10)
            {
                error = "The new password must be at least 10 characters.";
                return false;
            }
            var roleChanged = account.Role != role;
            account.FullName = fullName;
            account.Email = email;
            if (account.Username.Contains('@')) account.Username = email;
            account.Role = role;
            if (!string.IsNullOrEmpty(newPassword))
            {
                account.PasswordHash = hasher.HashPassword(account, newPassword);
                account.MustChangePassword = true;
            }
            if (roleChanged || !string.IsNullOrEmpty(newPassword))
                account.SessionVersion = Guid.NewGuid().ToString("N");
            Save();
            return true;
        }
    }

    public bool DeleteStaff(Guid id, out string error)
    {
        error = "";
        lock (gate)
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);
            if (account is null || account.Role != "User")
            {
                error = "Only staff accounts can be deleted.";
                return false;
            }
            accounts.Remove(account);
            Save();
            return true;
        }
    }

    public bool ChangeOwnPassword(Guid id, string currentPassword, string newPassword, out string error)
    {
        error = "";
        lock (gate)
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);
            if (account is null ||
                hasher.VerifyHashedPassword(account, account.PasswordHash, currentPassword)
                    == PasswordVerificationResult.Failed)
            {
                error = "Current password is incorrect.";
                return false;
            }
            if (newPassword.Length < 10 || newPassword == currentPassword)
            {
                error = "Use a new password of at least 10 characters.";
                return false;
            }
            account.PasswordHash = hasher.HashPassword(account, newPassword);
            account.MustChangePassword = false;
            account.SessionVersion = Guid.NewGuid().ToString("N");
            Save();
            return true;
        }
    }
}
