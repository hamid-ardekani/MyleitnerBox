using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.Users;

/// <summary>
/// توکن تازه‌سازی: رشتهٔ تصادفی که فقط hash آن ذخیره می‌شود. با هر استفاده «چرخانده» می‌شود (قبلی باطل، یکی جدید در همان <see cref="FamilyId"/>)؛
/// اگر توکن باطل‌شده دوباره بیاید (دزدیده‌شده) کل خانواده باطل می‌شود.
/// </summary>
public class RefreshToken : AggregateRoot<long>
{
    public long UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public Guid FamilyId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public static RefreshToken Issue(long userId, string tokenHash, Guid familyId, TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;
        return new RefreshToken { UserId = userId, TokenHash = tokenHash, FamilyId = familyId, CreatedAt = now, ExpiresAt = now + lifetime };
    }

    public bool IsRevoked => RevokedAt != null;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }
}
