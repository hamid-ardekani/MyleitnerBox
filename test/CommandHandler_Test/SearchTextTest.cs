using MyLeitner.Application.Flashcards.Queries;
using Should;

namespace CommandHandler_Test;

public class SearchTextTest
{
    [Fact]
    public void Arabic_yeh_and_kaf_become_persian()
    {
        SearchText.Normalize("كتاب عربي").ShouldEqual("کتاب عربی");
    }

    [Fact]
    public void Half_space_is_dropped_so_every_spelling_matches()
    {
        // نمی‌خواهم -> نمیخواهم
        SearchText.Normalize("نمی‌خواهم").ShouldEqual("نمیخواهم");
    }

    [Fact]
    public void Diacritics_are_removed_and_latin_is_lower_case()
    {
        SearchText.Normalize("مُرور Merge").ShouldEqual("مرور merge");
    }

    [Fact]
    public void Words_are_split_deduplicated_and_capped()
    {
        SearchText.Words("  merge  Merge abort ").ShouldEqual(new List<string> { "merge", "abort" });
        SearchText.Words(string.Join(' ', Enumerable.Range(1, 20).Select(i => $"w{i}"))).Count.ShouldEqual(8);
    }
}
