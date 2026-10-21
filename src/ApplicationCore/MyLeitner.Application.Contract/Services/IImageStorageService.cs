namespace MyLeitner.Application.Contract.Services;

public interface IImageStorageService
{
    /// <summary>یک تصویر را ذخیره می‌کند و مسیر URL آن را برای src تگ <c>&lt;img&gt;</c> برمی‌گرداند.</summary>
    /// <param name="imageData">data URI (<c>data:image/png;base64,...</c>) یا محتوای خام base64 تصویر.</param>
    Task<string> SaveImageAsync(string imageData, CancellationToken cancellationToken);
}
