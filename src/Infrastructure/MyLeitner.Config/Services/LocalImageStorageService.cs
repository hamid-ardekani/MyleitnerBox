using System.Text.RegularExpressions;
using MyLeitner.Application.Contract.Services;

namespace MyLeitner.Config.Services;

/// <summary>
/// تصویرهای بلوکی فلش‌کارت را در پوشه‌ای محلی ذخیره می‌کند و زیر <c>/uploads/flashcards</c> به‌صورت static سرو می‌شوند.
/// <paramref name="rootPath"/> قابل تنظیم است ("ImageStorage:RootPath") تا production بتواند پوشه‌ای بیرون از پوشهٔ deploy بدهد.
/// بعداً می‌شود کل کلاس را با blob storage عوض کرد؛ فراخوان‌ها فقط به <see cref="IImageStorageService"/> وابسته‌اند.
/// </summary>
public class LocalImageStorageService(string rootPath) : IImageStorageService
{
    private static readonly Dictionary<string, string> MimeToExtension = new()
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp"
    };

    private static readonly Regex DataUriPattern = new(@"^data:(?<mime>image/[a-zA-Z0-9.+-]+);base64,(?<data>.+)$", RegexOptions.Singleline);

    public async Task<string> SaveImageAsync(string imageData, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageData))
            throw new ArgumentException("Image data is empty.", nameof(imageData));

        var extension = ".png";
        var base64 = imageData;

        var match = DataUriPattern.Match(imageData);
        if (match.Success)
        {
            var mime = match.Groups["mime"].Value.ToLowerInvariant();
            if (!MimeToExtension.TryGetValue(mime, out extension!))
                throw new ArgumentException($"Unsupported image type '{mime}'.", nameof(imageData));

            base64 = match.Groups["data"].Value;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Image data is not valid base64.", nameof(imageData));
        }

        Directory.CreateDirectory(rootPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(rootPath, fileName);

        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);

        return $"/uploads/flashcards/{fileName}";
    }
}
