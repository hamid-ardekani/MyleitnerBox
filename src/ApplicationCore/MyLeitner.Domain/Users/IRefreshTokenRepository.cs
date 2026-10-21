namespace MyLeitner.Domain.Users;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct);

    /// <summary>همهٔ توکن‌های هنوز-باطل‌نشدهٔ یک خانواده.</summary>
    Task<List<RefreshToken>> GetActiveInFamilyAsync(Guid familyId, CancellationToken ct);

    Task AddAsync(RefreshToken token, CancellationToken ct);
}
