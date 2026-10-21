namespace MyLeitner.Identity;

public static class Errors
{
    public const string UserIsNotAuthorized = "User can not be authorized for this action.";
    public const string EmailVerifiedIsNotInUserClaims = "Email is not verified for user.";
    public const string UserIsNotAuthenticated ="User is not logged in."; 
}