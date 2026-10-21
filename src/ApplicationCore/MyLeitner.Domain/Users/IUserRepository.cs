namespace MyLeitner.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByExternalAsync(string provider, string subject, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
