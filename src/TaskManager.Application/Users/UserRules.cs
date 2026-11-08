using System.Text.RegularExpressions;

namespace TaskManager.Application.Users;

internal static partial class UserRules
{
    public const int MaxEmailLength = 320;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 200;

    public static Outcome<string> NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Outcome<string>.Fail(Failure.Validation("Email is required."));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > MaxEmailLength || !EmailPattern().IsMatch(normalized))
        {
            return Outcome<string>.Fail(Failure.Validation("Email is not valid."));
        }

        return Outcome<string>.Success(normalized);
    }

    public static Failure? ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return Failure.Validation("Password is required.");
        }

        if (password.Length > MaxPasswordLength)
        {
            return Failure.Validation($"Password must be {MaxPasswordLength} characters or fewer.");
        }

        var longEnough = password.Length >= MinPasswordLength;
        var hasLetter = password.Any(char.IsLetter);
        var hasDigit = password.Any(char.IsDigit);
        if (!longEnough || !hasLetter || !hasDigit)
        {
            return Failure.Validation("Password must be at least 8 characters and include a letter and a digit.");
        }

        return null;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();
}
