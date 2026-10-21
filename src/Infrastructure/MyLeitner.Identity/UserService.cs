using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyLeitner.Domain.Users;

namespace MyLeitner.Identity;

public class UserService : IUserService
{
    private const string Role = "role";
    private const string Sub = "sub";
    private const string ClientRole = "MyLeitner.Client";
    public const string FullName = "fullname";
    public const string Email = "email";
    public const string PhoneNumber = "phone_number";
    public const string AvatarUrl = "picture";
    public const string PhoneNumberVerified = "phone_number_verified";
    public const string EmailVerified = "email_verified";
    
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext is { User.Identity.IsAuthenticated: true };
    
    public bool IsSuperUser => IsAuthenticated
        && bool.TryParse(FindClaimValue(JwtTokenService.SuperUserClaim), out var isSuper) && isSuper;

    public bool IsAdmin => IsSuperUser;
    public bool IsClient => IsInRole(ClientRole);

    public long Id
    {
        get
        {
            var claim = GetClaim(Sub);
            if (claim != null && long.TryParse(claim.Value, out var id))
                return id;
            return 0;
        }
    }

    public long GetUserId() => Id == 0
            ? throw new IdentityException(Errors.UserIsNotAuthenticated)
            : Id;

    public bool ConfirmIsAuthenticated() => IsAuthenticated
            ? true
            : throw new IdentityException(Errors.UserIsNotAuthorized);

    public string FindFullName() => FindClaimValue(FullName);

    public string FindEmail() => FindClaimValue(Email);

    public string FindPhoneNumber() => FindClaimValue(PhoneNumber);

    public bool? FindPhoneNumberVerified() => bool.TryParse(FindClaimValue(PhoneNumberVerified), out var phoneNumberVerified) 
        ? phoneNumberVerified 
        : null;

    public bool? FindEmailVerified() => bool.TryParse(FindClaimValue(EmailVerified), out var emailVerified) 
            ? emailVerified 
            : null;

    public bool GetEmailVerified() => FindEmailVerified()
                                      ?? throw new IdentityException(Errors.EmailVerifiedIsNotInUserClaims);

    public bool IsInRole(string role) => GetClaim(Role, role) != null;
    
    public int NumberCategoriesUserCanAdded()
    {
        return IsSuperUser ? int.MaxValue : IsClient ? 5 : 1;
    }
    public int NumberBoxesUserCanAdded()
    {
        return IsSuperUser ? int.MaxValue : IsClient ? 100 : 5;
    }

    public int NumberFlashcardsUserCanAdded()
    {
        return IsSuperUser ? int.MaxValue : IsClient ? 1000 : 100;
    }

    public async Task<bool> CanUserAddFlashcardToBoxAsync(long boxId, long userId)
    {
        return true;
    }

    private string FindClaimValue(string claimName)
    {
        if (_httpContextAccessor.HttpContext == null)
            return string.Empty;

        var claim = _httpContextAccessor
            .HttpContext
            .User
            .Claims.FirstOrDefault(c => c.Type.Equals(claimName, StringComparison.CurrentCultureIgnoreCase));

        return claim != null
            ? claim.Value
            : string.Empty;
    }
    private Claim? GetClaim(string claimType, string claimValue)
    {
        return _httpContextAccessor
            .HttpContext
            ?.User
            .Claims
            .FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.CurrentCultureIgnoreCase) && c.Value == claimValue);
    }
    private Claim? GetClaim(string claimType)
    {
        return _httpContextAccessor
            .HttpContext
            ?.User
            .Claims
            .FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.CurrentCultureIgnoreCase));
    }
 
}