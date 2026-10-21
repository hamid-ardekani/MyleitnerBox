using Dela.Toolkit.Application;
using Dela.Toolkit.ServiceHost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLeitner.Identity;

namespace MyLeitner.Api.Controllers;

public record RegisterRequest(string Email, string Password, string? DisplayName, string? InviteCode);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

/// <summary>ثبت‌نام و ورود با ایمیل/رمز؛ فقط به دیتابیس خودمان تکیه دارد.</summary>
[AllowAnonymous]
public class AuthController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IAuthService auth)
    : BaseController(queryDispatcher, commandDispatcher)
{
    [HttpPost("Register")]
    public async Task<ActionResult> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request.Email, request.Password, request.DisplayName ?? "", request.InviteCode ?? "", ct);
        return ToResponse(result);
    }

    [HttpPost("Login")]
    public async Task<ActionResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request.Email, request.Password, ct);
        return ToResponse(result);
    }

    /// <summary>توکن تازه‌سازی را با توکن جدید عوض می‌کند؛ نامعتبر یا منقضی = ۴۰۱.</summary>
    [HttpPost("Refresh")]
    public async Task<ActionResult> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, ct);
        return ToResponse(result);
    }

    [HttpPost("Logout")]
    public async Task<ActionResult> LogoutAsync(RefreshRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return Ok();
    }

    private ActionResult ToResponse(AuthResult result)
    {
        if (result.Succeeded)
            return Ok(new { token = result.Token, refreshToken = result.RefreshToken, user = result.User });

        return result.Error switch
        {
            AuthService.RegistrationClosed => StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error }),
            AuthService.EmailTaken => Conflict(new { error = result.Error }),
            AuthService.InvalidCredentials or AuthService.InvalidRefresh => Unauthorized(new { error = result.Error }),
            _ => BadRequest(new { error = result.Error })
        };
    }
}
