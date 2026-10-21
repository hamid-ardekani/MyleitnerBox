namespace MyLeitner.Application.Contract.Dtos;

/// <param name="Step">گام لایتنری کارت (۰ جدید، ۱..۸).</param>
/// <param name="DaysLate">چند روز از موعدش گذشته (۰ = سر موعد)؛ null برای کارت جدید.</param>
public sealed record CardDto(long CardId, long BoxId, string Front, string Back, int Step = 0, int? DaysLate = null, string BoxName = "");