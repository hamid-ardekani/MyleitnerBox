namespace MyLeitner.Domain.Users;

public interface IUserDomainService
{
    Task<bool> CanUserAddBox(long userId, int box);
    Task<bool> CanUserAddFlashcardToBoxAsync(long userId,long boxId);
    Task<bool> CanUserAddCategory(long userId, object categoriesUserAddedCount);
    Task<bool> CanUserAddBoxIntoCategory(long userId, List<long> boxIds);
}

public class UserDomainService : IUserDomainService
{
    public Task<bool> CanUserAddBox(long userId,int box)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CanUserAddFlashcardToBoxAsync(long userId, long boxId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CanUserAddCategory(long userId, object categoriesUserAddedCount)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CanUserAddBoxIntoCategory(long userId, List<long> boxIds)
    {
        return Task.FromResult(true);
    }
}