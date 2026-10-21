using System.Security.Cryptography;
using System.Text;
using Dela.Toolkit.Domain;
using Microsoft.Extensions.Options;
using MyLeitner.Domain.Users;

namespace MyLeitner.Identity;

public record AuthUserDto(long Id, string Email, string DisplayName, bool IsSuperUser);

public record AuthResult(bool Succeeded, string? Error, string? Token, string? RefreshToken, AuthUserDto? User)
{
    public static AuthResult Fail(string error) => new(false, error, null, null, null);
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string displayName, string inviteCode, CancellationToken ct);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct);

    /// <summary>توکن تازه‌سازی را با یکی جدید عوض می‌کند (rotation) و توکن دسترسی تازه می‌دهد.</summary>
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct);

    /// <summary>خروج: همهٔ توکن‌های تازه‌سازیِ همان نشست باطل می‌شود. توکن نامعتبر هم خطا نمی‌دهد.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken ct);
}

/// <summary>ثبت‌نام، ورود و تازه‌سازی نشست؛ فقط به دیتابیس خودمان نیاز دارد، پس با قطع اینترنت هم کار می‌کند.</summary>
public class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IJwtTokenService tokens,
    IOptions<JwtOptions> options,
    IOptions<RegistrationOptions> registration) : IAuthService
{
    public const int MinPasswordLength = 8;

    public const string InvalidEmail = "invalid_email";
    public const string WeakPassword = "weak_password";
    public const string EmailTaken = "email_taken";
    public const string InvalidCredentials = "invalid_credentials";
    public const string InvalidRefresh = "invalid_refresh";
    public const string RegistrationClosed = "registration_closed";
    public const string InvalidInvite = "invalid_invite";

    /// <summary>
    /// دو درخواست هم‌زمان (مثلاً دو تب) ممکن است با یک توکن تازه‌سازی بیایند؛ توکنی که همین چند ثانیه پیش چرخانده شده
    /// «دزدی» حساب نمی‌شود و فقط رد می‌شود (کلاینت توکن تازه را از تب دیگر برمی‌دارد).
    /// </summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(30);

    public async Task<AuthResult> RegisterAsync(string email, string password, string displayName, string inviteCode, CancellationToken ct)
    {
        var expected = registration.Value.InviteCode;
        if (string.IsNullOrWhiteSpace(expected))
            return AuthResult.Fail(RegistrationClosed);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(inviteCode?.Trim() ?? ""), Encoding.UTF8.GetBytes(expected.Trim())))
            return AuthResult.Fail(InvalidInvite);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > User.EmailMaxLength)
            return AuthResult.Fail(InvalidEmail);
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            return AuthResult.Fail(WeakPassword);

        var name = string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName.Trim();
        if (name.Length > User.DisplayNameMaxLength)
            name = name[..User.DisplayNameMaxLength];

        var hash = PasswordHasher.Hash(password);
        var user = await users.GetByEmailAsync(email, ct);

        if (user == null)
        {
            user = User.Register(email, name, hash);
            await users.AddAsync(user, ct);
        }
        else if (user.PasswordHash == null && user.ExternalProvider == null)
        {
            // حساب از قبل ساخته‌شده (مثل کاربر ۱) که هنوز رمز ندارد: اولین ثبت‌نام با همین ایمیل صاحبش می‌شود.
            user.SetPasswordHash(hash);
        }
        else
        {
            return AuthResult.Fail(EmailTaken);
        }

        // کاربر تازه باید اول ذخیره شود تا Id داشته باشد
        await unitOfWork.CommitAsync(ct);
        return await StartSessionAsync(user, ct);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = string.IsNullOrWhiteSpace(email) ? null : await users.GetByEmailAsync(email, ct);

        // اگر کاربر نبود هم یک hash بسنج تا زمان پاسخ، وجود ایمیل را لو ندهد
        var ok = PasswordHasher.Verify(password ?? "", user?.PasswordHash ?? DummyHash);
        return user is { PasswordHash: not null } && ok
            ? await StartSessionAsync(user, ct)
            : AuthResult.Fail(InvalidCredentials);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthResult.Fail(InvalidRefresh);

        var stored = await refreshTokens.GetByHashAsync(HashToken(refreshToken), ct);
        if (stored == null)
            return AuthResult.Fail(InvalidRefresh);

        if (stored.IsRevoked)
        {
            // باطل‌شده‌ای که مدتی بعد دوباره آمده = احتمالاً دزدیده شده؛ کل نشست باطل می‌شود
            if (DateTime.UtcNow - stored.RevokedAt!.Value > RotationGrace)
            {
                foreach (var t in await refreshTokens.GetActiveInFamilyAsync(stored.FamilyId, ct))
                    t.Revoke();
                await unitOfWork.CommitAsync(ct);
            }
            return AuthResult.Fail(InvalidRefresh);
        }

        if (stored.IsExpired)
            return AuthResult.Fail(InvalidRefresh);

        var user = await users.GetByIdAsync(stored.UserId, ct);
        if (user == null)
            return AuthResult.Fail(InvalidRefresh);

        stored.Revoke();
        var next = await IssueRefreshTokenAsync(user.Id, stored.FamilyId, ct);
        await unitOfWork.CommitAsync(ct);
        return Success(user, next);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var stored = await refreshTokens.GetByHashAsync(HashToken(refreshToken), ct);
        if (stored == null)
            return;

        foreach (var t in await refreshTokens.GetActiveInFamilyAsync(stored.FamilyId, ct))
            t.Revoke();
        await unitOfWork.CommitAsync(ct);
    }

    private static readonly string DummyHash = PasswordHasher.Hash("dummy-password");

    private async Task<AuthResult> StartSessionAsync(User user, CancellationToken ct)
    {
        var raw = await IssueRefreshTokenAsync(user.Id, Guid.NewGuid(), ct);
        await unitOfWork.CommitAsync(ct);
        return Success(user, raw);
    }

    /// <summary>توکن تازه می‌سازد و (بدون commit) اضافه می‌کند؛ مقدار خام فقط همین‌جا و برای کلاینت وجود دارد.</summary>
    private async Task<string> IssueRefreshTokenAsync(long userId, Guid familyId, CancellationToken ct)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var lifetime = TimeSpan.FromDays(options.Value.RefreshTokenDays);
        await refreshTokens.AddAsync(RefreshToken.Issue(userId, HashToken(raw), familyId, lifetime), ct);
        return raw;
    }

    private static string HashToken(string raw) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    private AuthResult Success(User user, string refreshToken) =>
        new(true, null, tokens.Create(user), refreshToken, new AuthUserDto(user.Id, user.Email, user.DisplayName, user.IsSuperUser));
}
