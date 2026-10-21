using Autofac;
using CommandHandler_Test.Autofac;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using Should;

namespace CommandHandler_Test;

public class GetNextCardInCategoryTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ReadModelDbContext _readContext;

    public GetNextCardInCategoryTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _cardReadRepository = scope.Resolve<ICardReadRepository>();
        _readContext = scope.Resolve<ReadModelDbContext>();
    }

    private async Task<(long CategoryId, long DueFlashcardId)> SeedCategoryWithFlashcardsAsync(DateTime? dueFlashcardNextAnswerDate, short dailyNewCardLimit = 10)
    {
        var box = new Box { Name = "box", OrderBox = 1, ReadFlashcardAloud = false, UserId = 1, DailyNewCardLimit = dailyNewCardLimit, DailyBacklogFloor = 20 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var category = new Category { Name = "category", Order = 1, UserId = 1 };
        _readContext.Categories.Add(category);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.CategoryBoxes.Add(new CategoryBox { Order = 1, BoxId = box.Id, CategoryId = category.Id });
        await _readContext.SaveChangesAsync(_cancellationToken);

        var dueFlashcard = new Flashcard { Order = 1, Front = "due front", Back = "due back", Tags = "t", BoxId = box.Id };
        var notDueFlashcard = new Flashcard { Order = 2, Front = "not due front", Back = "not due back", Tags = "t", BoxId = box.Id };
        _readContext.Flashcards.AddRange(dueFlashcard, notDueFlashcard);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.LearningSchedules.AddRange(
            new LearningSchedule { FlashcardId = dueFlashcard.Id, UserId = 1, Stability = 1, Difficulty = 0.3, Interval = 1, NextAnswerDate = dueFlashcardNextAnswerDate },
            new LearningSchedule { FlashcardId = notDueFlashcard.Id, UserId = 1, Stability = 1, Difficulty = 0.3, Interval = 1, NextAnswerDate = DateTime.UtcNow.AddDays(5) });
        await _readContext.SaveChangesAsync(_cancellationToken);

        return (category.Id, dueFlashcard.Id);
    }

    [Fact]
    public async Task GetDueCardsInCategory_IncludesCardWithPastNextAnswerDate_ExcludesFutureOne()
    {
        var (categoryId, dueFlashcardId) = await SeedCategoryWithFlashcardsAsync(DateTime.UtcNow.AddDays(-1));

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);

        due.Count.ShouldEqual(1);
        due[0].CardId.ShouldEqual(dueFlashcardId);
    }

    [Fact]
    public async Task GetDueCardsInCategory_TreatsNeverAnsweredCard_AsDue()
    {
        // کارت تازه که هیچ‌وقت AnswerProcess نخورده، NextAnswerDate=null دارد و باید
        // بلافاصله موعدرسیده حساب شود - نه این‌که هیچ‌وقت دیده نشود.
        var (categoryId, dueFlashcardId) = await SeedCategoryWithFlashcardsAsync(null);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);

        due.Count.ShouldEqual(1);
        due[0].CardId.ShouldEqual(dueFlashcardId);
    }

    [Fact]
    public async Task GetDueCardsInCategory_AdmitsNewCardsOnlyUpToTheBoxDailyLimit_InBoxOrder()
    {
        var (categoryId, firstNewId) = await SeedCategoryWithFlashcardsAsync(null, dailyNewCardLimit: 2);
        var boxId = (await _readContext.Flashcards.FindAsync(firstNewId))!.BoxId;

        // سه کارت جدید دیگر بعد از اولی (Order 3..5)؛ با سقف ۲ فقط Order 1 و 3 باید بیایند
        var extra = Enumerable.Range(3, 3)
            .Select(order => new Flashcard { Order = (short)order, Front = $"new {order}", Back = "b", Tags = "t", BoxId = boxId })
            .ToList();
        _readContext.Flashcards.AddRange(extra);
        await _readContext.SaveChangesAsync(_cancellationToken);
        _readContext.LearningSchedules.AddRange(extra.Select(f => new LearningSchedule { FlashcardId = f.Id, UserId = 1 }));
        await _readContext.SaveChangesAsync(_cancellationToken);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);

        due.Select(c => c.CardId).ShouldEqual(new[] { firstNewId, extra[0].Id });
    }

    [Fact]
    public async Task GetDueCardsInCategory_CountsTheWholeDay_NotTheExactHour()
    {
        // موعد روزانه است: کارتی که موعدش چند ساعت دیگرِ همین امروز (محلی) است، از همین حالا در مرور می‌آید
        var laterToday = MyLeitner.Domain.StudySessions.StudySessionClock.UtcStartOf(
            MyLeitner.Domain.StudySessions.StudySessionClock.LocalToday(DateTime.UtcNow).AddDays(1)).AddMinutes(-1);
        var (categoryId, dueFlashcardId) = await SeedCategoryWithFlashcardsAsync(laterToday);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);

        due.Select(c => c.CardId).ShouldEqual(new[] { dueFlashcardId });
    }

    private async Task AddWrongAnswersTodayAsync(long boxId, int count, bool reviews)
    {
        var now = DateTime.UtcNow;
        _readContext.ReviewLogs.AddRange(Enumerable.Range(0, count).Select(i => new ReviewLog
        {
            UserId = 1,
            FlashcardId = 900_000 + i,
            BoxId = boxId,
            AnswerTypeId = MyLeitner.Domain.Flashcards.AnswerTypeEnum.Wrong.Id,
            AnsweredAt = now,
            ReviewDate = MyLeitner.Domain.StudySessions.StudySessionClock.LocalToday(now),
            PreviousAnsweredAt = reviews ? now.AddDays(-3) : null,
        }));
        await _readContext.SaveChangesAsync(_cancellationToken);
    }

    [Fact]
    public async Task SixWrongFirstAnswersOnNewCards_DoNotStopNewCards()
    {
        var (categoryId, newCardId) = await SeedCategoryWithFlashcardsAsync(null);
        var boxId = (await _readContext.Flashcards.FindAsync(newCardId))!.BoxId;

        await AddWrongAnswersTodayAsync(boxId, 6, reviews: false);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);
        due.Select(c => c.CardId).ShouldContain(newCardId);
    }

    [Fact]
    public async Task SixWrongReviews_StopNewCardsForToday()
    {
        var (categoryId, newCardId) = await SeedCategoryWithFlashcardsAsync(null);
        var boxId = (await _readContext.Flashcards.FindAsync(newCardId))!.BoxId;

        await AddWrongAnswersTodayAsync(boxId, 6, reviews: true);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);
        due.Select(c => c.CardId).ShouldNotContain(newCardId);
    }

    [Fact]
    public async Task OldBacklog_OnlyTheBoxFloorComesUp_MostOverdueFirst_WhileTodaysDueAllCome()
    {
        var (categoryId, dueTodayId) = await SeedCategoryWithFlashcardsAsync(DateTime.UtcNow);
        var box = await _readContext.Boxes.FindAsync((await _readContext.Flashcards.FindAsync(dueTodayId))!.BoxId);
        box!.DailyBacklogFloor = 2;

        // پنج کارت با ۱ تا ۵ روز تأخیر
        var overdue = Enumerable.Range(1, 5)
            .Select(days => new Flashcard { Order = (short)(10 + days), Front = $"late {days}", Back = "b", Tags = "t", BoxId = box.Id })
            .ToList();
        _readContext.Flashcards.AddRange(overdue);
        await _readContext.SaveChangesAsync(_cancellationToken);
        _readContext.LearningSchedules.AddRange(overdue.Select((f, i) => new LearningSchedule
        {
            FlashcardId = f.Id, UserId = 1, Stability = 4, Difficulty = 0.3, Interval = 4, NextAnswerDate = DateTime.UtcNow.AddDays(-(i + 1)),
        }));
        await _readContext.SaveChangesAsync(_cancellationToken);

        var due = await _cardReadRepository.GetDueCardsInCategory(categoryId, _cancellationToken);

        // کارت امروز + دو کارت با بیشترین تأخیر (۵ و ۴ روز)
        due.Select(c => c.CardId).ShouldEqual(new[] { dueTodayId, overdue[4].Id, overdue[3].Id });
    }

    [Fact]
    public async Task GetCardContentAsync_ReturnsFrontAndBack()
    {
        var (_, dueFlashcardId) = await SeedCategoryWithFlashcardsAsync(DateTime.UtcNow.AddDays(-1));

        var content = await _cardReadRepository.GetCardContentAsync(dueFlashcardId, _cancellationToken);

        content.ShouldNotBeNull();
        content!.Front.ShouldEqual("due front");
        content.Back.ShouldEqual("due back");
    }
}
