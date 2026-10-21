using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.Users;

/// <summary>
/// کاربر برنامه. ورود اصلی با ایمیل و رمز ذخیره‌شده روی همین سرور است (بدون نیاز به اینترنت)؛
/// ورود با Google اختیاری است و در <see cref="ExternalProvider"/>/<see cref="ExternalSubject"/> ثبت می‌شود.
/// </summary>
public class User : AggregateRoot<long>
{
    public const int EmailMaxLength = 256;
    public const int DisplayNameMaxLength = 100;

    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? ExternalProvider { get; private set; }
    public string? ExternalSubject { get; private set; }

    /// <summary>کاربر ویژه: به همه‌چیز دسترسی دارد و هیچ محدودیتی ندارد.</summary>
    public bool IsSuperUser { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static User Register(string email, string displayName, string passwordHash) => new()
    {
        Email = Normalize(email),
        DisplayName = displayName.Trim(),
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow
    };

    public static User RegisterExternal(string email, string displayName, string provider, string subject) => new()
    {
        Email = Normalize(email),
        DisplayName = displayName.Trim(),
        ExternalProvider = provider,
        ExternalSubject = subject,
        CreatedAt = DateTime.UtcNow
    };

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void LinkExternal(string provider, string subject)
    {
        ExternalProvider = provider;
        ExternalSubject = subject;
    }

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
