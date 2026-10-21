using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Users;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class UserRepository(MyLeitnerDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(long id, CancellationToken ct) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = User.Normalize(email);
        return context.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> GetByExternalAsync(string provider, string subject, CancellationToken ct) =>
        context.Users.FirstOrDefaultAsync(u => u.ExternalProvider == provider && u.ExternalSubject == subject, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
    }
}
