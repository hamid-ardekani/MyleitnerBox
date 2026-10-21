using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.StudySessions;
using Should;

namespace CommandHandler_Test;

public class StudySessionCardSelectorTest
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly DateTime Now = DateTime.UtcNow;

    private static StudySession NewSession() => StudySession.ForAdd(1, StudySessionScopeType.Category, 10, Today);

    [Fact]
    public void SelectNext_PrefersNeverShownCard_OverDeferredCard_EvenWithLowerPriority()
    {
        var session = NewSession();

        // کارت ۳۰۰ همین امروز Show Later خورده - با اینکه سختی بالایی دارد
        session.ShowFlashcard(300);
        session.ResolveCurrentAsDeferred(flashcardId: 300, boxId: 5, shownAt: Now, resolvedAt: Now);

        var deferredButHardCandidate = new CardCandidate(300, boxId: 5, difficulty: 1.0, stability: 1, nextAnswerDate: Now);
        var neverShownButEasyCandidate = new CardCandidate(301, boxId: 6, difficulty: 0.1, stability: 100, nextAnswerDate: Now);

        var next = StudySessionCardSelector.SelectNext(session, [deferredButHardCandidate, neverShownButEasyCandidate], Now);

        next.ShouldNotBeNull();
        next!.CardId.ShouldEqual(301L);
    }

    [Fact]
    public void SelectNext_ExcludesCardsAlreadyAnsweredToday()
    {
        var session = NewSession();

        session.ShowFlashcard(500);
        session.ResolveCurrentAsAnswered(flashcardId: 500, boxId: 1, AnswerTypeEnum.Confident, shownAt: Now, resolvedAt: Now);

        // کارت ۵۰۰ عمداً سختی خیلی بالایی دارد تا اگر فیلتر نشود، برنده شود
        var alreadyAnswered = new CardCandidate(500, boxId: 1, difficulty: 1.0, stability: 1, nextAnswerDate: Now);
        var stillDue = new CardCandidate(501, boxId: 2, difficulty: 0.2, stability: 50, nextAnswerDate: Now);

        var next = StudySessionCardSelector.SelectNext(session, [alreadyAnswered, stillDue], Now);

        next.ShouldNotBeNull();
        next!.CardId.ShouldEqual(501L);
    }

    [Fact]
    public void SelectNext_PenalizesConsecutiveCardsFromSameBox_ToPreventStarvation()
    {
        var session = NewSession();

        // سه پاسخ پشت‌سرهم از باکس ۱ - یعنی الان دقیقاً به سقف MaxConsecutivePerBox رسیده‌ایم
        for (var i = 0; i < 3; i++)
        {
            session.ShowFlashcard(100 + i);
            session.ResolveCurrentAsAnswered(flashcardId: 100 + i, boxId: 1, AnswerTypeEnum.Confident, shownAt: Now, resolvedAt: Now.AddMinutes(i));
        }

        // کارت باکس ۱ عمداً سختی خیلی بالاتری دارد - بدون پنالتی باید برنده شود
        var sameBoxHighPriority = new CardCandidate(200, boxId: 1, difficulty: 0.95, stability: 1, nextAnswerDate: Now);
        var otherBoxLowerPriority = new CardCandidate(201, boxId: 2, difficulty: 0.3, stability: 5, nextAnswerDate: Now);

        var next = StudySessionCardSelector.SelectNext(session, [sameBoxHighPriority, otherBoxLowerPriority], Now);

        next.ShouldNotBeNull();
        next!.CardId.ShouldEqual(201L, "بعد از ۳ بار پشت‌سرهم از یک باکس، پنالتی نباید بگذارد همان باکس دوباره انتخاب شود.");
    }

    [Fact]
    public void SelectNext_AmongUntouchedCards_PicksHigherPriorityFirst()
    {
        var session = NewSession();

        var harderCard = new CardCandidate(400, boxId: 1, difficulty: 0.9, stability: 1, nextAnswerDate: Now);
        var easierCard = new CardCandidate(401, boxId: 1, difficulty: 0.1, stability: 100, nextAnswerDate: Now);

        var next = StudySessionCardSelector.SelectNext(session, [easierCard, harderCard], Now);

        next.ShouldNotBeNull();
        next!.CardId.ShouldEqual(400L);
    }

    [Fact]
    public void SelectNext_ReturnsNull_WhenNoCandidatesLeft()
    {
        var session = NewSession();

        var next = StudySessionCardSelector.SelectNext(session, [], Now);

        next.ShouldBeNull();
    }

    private static void Answer(StudySession session, long cardId, AnswerTypeEnum answer, int minute, bool wasNew = false)
    {
        session.ShowFlashcard(cardId);
        session.ResolveCurrentAsAnswered(cardId, boxId: 1, answer, shownAt: Now, resolvedAt: Now.AddMinutes(minute), wasNew);
    }

    private static CardCandidate Review(long id) => new(id, boxId: 1, difficulty: 0.5, stability: 5, nextAnswerDate: Now);
    private static CardCandidate NewCard(long id) => new(id, boxId: 1, difficulty: 0.3, stability: 0, nextAnswerDate: Now, isNew: true);

    [Fact]
    public void A_new_card_waits_while_reviews_remain_and_nothing_is_earned_yet()
    {
        var session = NewSession();
        for (var i = 0; i < 4; i++) Answer(session, 10 + i, AnswerTypeEnum.Confident, i);

        StudySessionCardSelector.SelectNext(session, [NewCard(900), Review(1)], Now)!.CardId.ShouldEqual(1L);
    }

    [Fact]
    public void Five_good_reviews_in_a_row_earn_a_new_card()
    {
        var session = NewSession();
        var answers = new[] { AnswerTypeEnum.Confident, AnswerTypeEnum.NeedMoreReview, AnswerTypeEnum.NotSure, AnswerTypeEnum.Confident, AnswerTypeEnum.Confident };
        for (var i = 0; i < 5; i++) Answer(session, 10 + i, answers[i], i); // average 0.85

        StudySessionCardSelector.SelectNext(session, [NewCard(900), Review(1)], Now)!.CardId.ShouldEqual(900L);
    }

    [Fact]
    public void A_wrong_answer_among_the_last_five_earns_nothing()
    {
        var session = NewSession();
        for (var i = 0; i < 4; i++) Answer(session, 10 + i, AnswerTypeEnum.Confident, i);
        Answer(session, 20, AnswerTypeEnum.Wrong, 5);

        StudySessionCardSelector.SelectNext(session, [NewCard(900), Review(1)], Now)!.CardId.ShouldEqual(1L);
    }

    [Fact]
    public void Lucky_guesses_do_not_earn_a_reward()
    {
        var session = NewSession();
        for (var i = 0; i < 5; i++) Answer(session, 10 + i, AnswerTypeEnum.UnCertain, i);

        StudySessionCardSelector.SelectNext(session, [NewCard(900), Review(1)], Now)!.CardId.ShouldEqual(1L);
    }

    [Fact]
    public void After_a_new_card_the_next_one_needs_five_fresh_reviews()
    {
        var session = NewSession();
        for (var i = 0; i < 5; i++) Answer(session, 10 + i, AnswerTypeEnum.Confident, i);
        Answer(session, 900, AnswerTypeEnum.Confident, 6, wasNew: true);
        for (var i = 0; i < 3; i++) Answer(session, 30 + i, AnswerTypeEnum.Confident, 7 + i);

        StudySessionCardSelector.SelectNext(session, [NewCard(901), Review(1)], Now)!.CardId.ShouldEqual(1L);
    }

    [Fact]
    public void Once_the_reviews_are_done_the_remaining_new_cards_come()
    {
        var session = NewSession();
        Answer(session, 10, AnswerTypeEnum.Wrong, 0);

        StudySessionCardSelector.SelectNext(session, [NewCard(900)], Now)!.CardId.ShouldEqual(900L);
    }

    private static void Defer(StudySession session, long cardId, int minute)
    {
        session.ShowFlashcard(cardId);
        session.ResolveCurrentAsDeferred(cardId, boxId: 1, shownAt: Now, resolvedAt: Now.AddMinutes(minute));
    }

    [Fact]
    public void Put_off_reviews_do_not_block_new_cards()
    {
        var session = NewSession();
        Defer(session, 1, 0);
        Defer(session, 2, 1);

        StudySessionCardSelector.SelectNext(session, [Review(1), Review(2), NewCard(900)], Now)!.CardId.ShouldEqual(900L);
    }

    [Fact]
    public void Show_later_never_brings_the_same_card_straight_back_when_others_are_left()
    {
        var session = NewSession();
        var hard = new CardCandidate(1, boxId: 1, difficulty: 0.9, stability: 1, nextAnswerDate: Now);
        var easy = new CardCandidate(2, boxId: 1, difficulty: 0.1, stability: 50, nextAnswerDate: Now);

        Defer(session, 2, 0); // put off first
        Defer(session, 1, 1); // the hard one just now

        // فقط کارت‌های «بعداً»خورده مانده‌اند: آن که زودتر رد شده، نه پراولویت‌ترینِ تازه‌ردشده
        StudySessionCardSelector.SelectNext(session, [hard, easy], Now)!.CardId.ShouldEqual(2L);
    }

    [Fact]
    public void A_new_card_the_user_added_today_comes_like_a_review_without_earning_it()
    {
        var session = NewSession();
        var added = new CardCandidate(900, boxId: 1, difficulty: 0.3, stability: 0, nextAnswerDate: Now, isNew: true, addedToReviewToday: true);

        // هنوز جایزه‌ای کسب نشده و یک مرور منتظر است: کارت جدید ساده نمی‌آید ولی کارت دستی‌اضافه‌شده می‌آید (اینجا سخت‌تر و قدیمی‌تر است)
        var hardReview = new CardCandidate(1, boxId: 1, difficulty: 0.1, stability: 100, nextAnswerDate: Now);
        StudySessionCardSelector.SelectNext(session, [added, hardReview], Now)!.CardId.ShouldEqual(900L);
    }
}
