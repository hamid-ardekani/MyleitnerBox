using System.Text;

namespace MyLeitner.Application.Flashcards.Queries;

/// <summary>
/// یکسان‌سازی متن برای جستجو: «ي/ى»→«ی»، «ك»→«ک»، حذف نیم‌فاصله و اعراب، حروف کوچک
/// (کارت‌های لایتنر قدیم اغلب حروف عربی دارند). سمت SQL همین کار با REPLACE روی ستون انجام می‌شود.
/// </summary>
public static class SearchText
{
    public const string ArabicYeh = "ي";
    public const string AlefMaksura = "ى";
    public const string PersianYeh = "ی";
    public const string ArabicKaf = "ك";
    public const string PersianKaf = "ک";
    public const string Zwnj = "‌";

    public static string Normalize(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch is >= 'ً' and <= 'ْ' or 'ٰ' or '‌')
                continue; // اعراب، نیم‌فاصله
            sb.Append(ch switch
            {
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                _ => char.ToLowerInvariant(ch),
            });
        }

        return sb.ToString();
    }

    /// <summary>کلمه‌های جستجو (یکسان‌شده، بدون تکرار، حداکثر ۸ تا).</summary>
    public static List<string> Words(string? text) =>
        Normalize(text).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().Take(8).ToList();

    private static readonly HashSet<string> StopWords = new()
    {
        "این", "برای", "است", "چیست", "چیه", "چه", "چرا", "کدام", "کدوم", "معنی", "یعنی", "های",
        "what", "which", "that", "this", "with", "from", "does", "are", "the", "and", "for", "how", "why",
    };

    /// <summary>
    /// کلمه‌های «معنادار» یک سؤال برای یافتن کارت مشابه: یکسان‌شده، بدون تکرار، حداقل ۳ حرف،
    /// بدون کلمه‌های رایج (چیست، the، ...)، حداکثر ۸ تا.
    /// </summary>
    public static List<string> SimilarityWords(string? text) =>
        Normalize(text).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => w.Trim('.', ',', ';', ':', '!', '?', '؟', '،', '؛', '(', ')', '"', '\''))
            .Where(w => w.Length >= 3 && !StopWords.Contains(w))
            .Distinct().Take(8).ToList();
}
