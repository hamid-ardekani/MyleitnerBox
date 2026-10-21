namespace MyLeitner.Identity;

public class RegistrationOptions
{
    public const string Section = "Registration";

    /// <summary>
    /// کد دعوتِ ثبت‌نام (مشترک). خالی = ثبت‌نام بسته است. بدون کد درست هیچ‌کس نمی‌تواند حساب بسازد یا حساب بدون‌رمزِ موجود
    /// (مثل کاربر ۱) را تصاحب کند.
    /// </summary>
    public string InviteCode { get; set; } = "";
}
