namespace MyLeitner.Identity;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "MyLeitner";
    public string Audience { get; set; } = "MyLeitner.Client";
    /// <summary>کلید امضای HMAC-SHA256 (حداقل ۳۲ کاراکتر). فقط روی سرور است؛ اعتبارسنجی توکن هیچ تماس بیرونی ندارد.</summary>
    public string Key { get; set; } = "";
    /// <summary>عمر توکن دسترسی؛ کوتاه نگه داشته می‌شود و با توکن تازه‌سازی نو می‌شود.</summary>
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 90;
}
