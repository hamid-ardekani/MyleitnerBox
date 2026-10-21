namespace MyLeitner.Common.Results;

public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // سازنده‌های خطاهای رایج
    public static Error NotFound(string code, string description) =>
        new NotFoundError(code, description);

    public static Error Validation(string code, string description) =>
        new ValidationError(code, description);

    public static Error Conflict(string code, string description) =>
        new ConflictError(code, description);

    public static Error Unauthorized(string code, string description) =>
        new UnauthorizedError(code, description);

    public static Error Failure(string code, string description) =>
        new FailureError(code, description);

    public static Error UnhandledError(string description = "") =>
        new UnhandledError(description);
}

// خطاهای نوع‌دار برای دسته‌بندی بهتر
public sealed record NotFoundError(string Code, string Description)
    : Error(Code, Description);

public sealed record ValidationError(string Code, string Description)
    : Error(Code, Description);

public sealed record ConflictError(string Code, string Description)
    : Error(Code, Description);

public sealed record UnauthorizedError(string Code, string Description)
    : Error(Code, Description);

public sealed record FailureError(string Code, string Description)
    : Error(Code, Description);

public sealed record UnhandledError(string Description)
    : Error("500", "Please call by administrator " + Description);