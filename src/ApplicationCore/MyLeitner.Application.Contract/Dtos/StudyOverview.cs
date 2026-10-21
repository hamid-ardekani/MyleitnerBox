namespace MyLeitner.Application.Contract.Dtos;

/// <summary>
/// سهم کارت جدید امروزِ یک جعبه و چرا (MyLeitner.Domain.Boxes.NewCardQuota) - برای پیام «تلاشت باعث شد…».
/// </summary>
/// <param name="BaseLimit">سقفی که کاربر گذاشته.</param>
/// <param name="Target">سهم امروز بعد از ضریب بار مرور و دقت.</param>
/// <param name="Waiting">کارت جدیدِ منتظر در جعبه (اگر صفر است، پیامی لازم نیست).</param>
/// <param name="RecentAccuracyPercent">درستیِ مرورهای ۷ روز اخیر؛ null یعنی کمتر از ۲۰ مرور.</param>
public record NewCardQuotaDto(
    int BaseLimit,
    int Target,
    int Allowed,
    int Waiting,
    int DueReviews,
    double WorkloadFactor,
    double AccuracyFactor,
    double? RecentAccuracyPercent,
    bool StoppedByWrongs);

/// <summary>
/// سهم امروزِ کارت‌های عقب‌افتاده (بدهی) یک جعبه (MyLeitner.Domain.Boxes.BacklogQuota).
/// </summary>
/// <param name="Released">چند کارت بدهی امروز در کل آزاد است (شامل صاف‌شده‌ها).</param>
/// <param name="Remaining">کل کارت‌های بدهیِ هنوز مانده.</param>
/// <param name="Available">چند کارت بدهی همین حالا در صف است.</param>
/// <param name="Pace">Warmup / Growing / Steady / Resting / Capped.</param>
/// <param name="MoreAvailable">چند کارت عقب‌افتادهٔ دیگر می‌شود در پایان مرور پیشنهاد داد (۰ = پیشنهادی نیست).</param>
public record BacklogQuotaDto(int Floor, int Released, int DoneToday, int Remaining, int Available, string Pace, double? RecentAccuracyPercent, int MoreAvailable = 0);

/// <summary>برنامهٔ امروز یک جعبه: سهم کارت جدید + سهم بدهی.</summary>
public record BoxTodayPlanDto(NewCardQuotaDto NewCards, BacklogQuotaDto Backlog);

/// <param name="Step">گام لایتنری: ۰ جدید، ۱..۸، ۹ یادگرفته‌شده (MyLeitner.Domain.LearningSchedules.LearningStep).</param>
public record StepCountDto(int Step, int Count);

/// <param name="Pending">کارت‌های امروزِ این جعبه که هنوز مانده‌اند (با سقف کارت جدید).</param>
/// <param name="AnsweredToday">کارت‌های متفاوتی از این جعبه که امروز جواب گرفته‌اند.</param>
/// <param name="WrongToday">تعداد پاسخ‌های «اشتباه» امروز در این جعبه.</param>
/// <param name="WrongLimit">بیش از این تعداد «اشتباه» در روز، این جعبه آن روز کارت جدید نمی‌آورد (Box.MaxTodayWrongForNewCards).</param>
public record StudyOverviewBoxDto(long BoxId, string Name, int Pending, int AnsweredToday, int WrongToday, List<StepCountDto> PendingBySteps, int WrongLimit, NewCardQuotaDto? NewCards = null, BacklogQuotaDto? Backlog = null);

/// <summary>
/// وضعیت امروزِ یک مرور (جعبه یا دسته‌بندی): کارت‌های مانده بر اساس گام فعلی‌شان، کارت‌های جواب‌داده بر اساس گامی که
/// الان در آن‌اند، و تفکیک جعبه‌ها. هر دو فهرست گام همهٔ گام‌های ۰ تا ۹ را دارند (با صفر).
/// </summary>
/// <param name="NextStep">گام کارتی که مرور با آن شروع/ادامه می‌شود (همان انتخاب الگوریتم)؛ null وقتی چیزی نمانده.</param>
public record StudyOverviewDto(
    int PendingTotal,
    List<StepCountDto> Pending,
    int AnsweredTotal,
    List<StepCountDto> Answered,
    List<StudyOverviewBoxDto> Boxes,
    int? NextStep);
