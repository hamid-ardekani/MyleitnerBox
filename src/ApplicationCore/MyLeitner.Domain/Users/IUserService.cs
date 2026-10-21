namespace MyLeitner.Domain.Users;

public interface IUserService
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    /// <summary>کاربر ویژه: همهٔ محدودیت‌ها برایش برداشته است.</summary>
    bool IsSuperUser { get; }
    
    bool IsClient { get; }
    long Id { get; }
    long GetUserId();
    bool ConfirmIsAuthenticated();
    string FindFullName();
    string FindPhoneNumber();
    bool? FindPhoneNumberVerified();
    bool? FindEmailVerified();
    bool GetEmailVerified();
    bool IsInRole(string role);
    int NumberCategoriesUserCanAdded();
    int NumberBoxesUserCanAdded();
    int NumberFlashcardsUserCanAdded();
    Task<bool> CanUserAddFlashcardToBoxAsync(long commandBoxId, long commandUserId);
}