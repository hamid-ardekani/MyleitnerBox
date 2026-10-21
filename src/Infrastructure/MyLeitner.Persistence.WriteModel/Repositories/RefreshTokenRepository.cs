using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Users;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class RefreshTokenRepository(MyLeitnerDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct) =>
        context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public Task<List<RefreshToken>> GetActiveInFamilyAsync(Guid familyId, CancellationToken ct) =>
        context.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null).ToListAsync(ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await context.RefreshTokens.AddAsync(token, ct);
    }
}
