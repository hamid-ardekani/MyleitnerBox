namespace MyLeitner.Application.Contract.Dtos;

/// <summary>
/// «دور دوم»: کارت‌هایی که آخرین جواب امروزشان «بلد نبودم» یا «شانسی» بوده، برای یک بار دیدن دوباره.
/// </summary>
/// <param name="Total">همهٔ کارت‌های واجد شرایط (قبل از سقف).</param>
/// <param name="WrongToday">چند «بلد نبودم» امروز در جعبه(های) این مرور - بیش از ۵ یعنی روز سخت (سقف ۵ و لحن ملایم‌تر).</param>
/// <param name="Items">حداکثر ۱۰ (روز سخت ۵)، «بلد نبودم»ها اول، بعد سخت‌ترها.</param>
public sealed record PracticeDto(int Total, int WrongToday, bool HardDay, List<PracticeCardDto> Items);

/// <summary>کارت‌های «دور دوم» امروز به تفکیک جعبه و دسته (فقط غیرصفرها).</summary>
public sealed record PracticeCountsDto(Dictionary<long, int> Boxes, Dictionary<long, int> Categories);

public sealed record PracticeCardDto(long CardId, long BoxId, string BoxName, string Front, string Back, short AnswerTypeId);
