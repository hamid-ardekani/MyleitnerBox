namespace MyLeitner.Domain.Boxes;

public interface IBoxRepository
{
    Task AddAsync(Box box);
    void Remove(Box box);
    Task<Box?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Box?> GetByOrderAsync(short order, CancellationToken cancellationToken);
    Task<int> GetBoxByUserIdCountAsync(long userId);
    Task<List<Box>> GetAllBoxesByUserIdAsync(long userId, CancellationToken cancellationToken);
}