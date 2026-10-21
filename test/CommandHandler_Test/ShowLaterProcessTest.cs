using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Learnings.Commands;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.StudySessions;
using Should;

namespace CommandHandler_Test;

public class ShowLaterProcessTest : IClassFixture<AutofacTestFixture>
{
    private const long UserId = 1;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly IBoxRepository _boxRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IStudySessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommandDispatcher _commandDispatcher;

    public ShowLaterProcessTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _boxRepository = scope.Resolve<IBoxRepository>();
        _flashcardRepository = scope.Resolve<IFlashcardRepository>();
        _sessionRepository = scope.Resolve<IStudySessionRepository>();
        _unitOfWork = scope.Resolve<IUnitOfWork>();
        _commandDispatcher = scope.Resolve<ICommandDispatcher>();
    }

    private async Task<(long BoxId, long FlashcardId)> SeedBoxWithFlashcardAsync()
    {
        var box = Box.BoxForAdd("show-later box", 1, false, UserId);
        await _boxRepository.AddAsync(box);
        await _unitOfWork.CommitAsync(_cancellationToken);

        var flashcard = Flashcard.ForAdd(box.Id, 1, "front", "back", "front", "back", ["tag"]);
        await _flashcardRepository.AddAsync(flashcard);
        await _unitOfWork.CommitAsync(_cancellationToken);

        return (box.Id, flashcard.Id);
    }

    [Fact]
    public async Task Handle_MovesCurrentFlashcard_ToDeferred_AndClearsCurrentFlashcardId()
    {
        var (boxId, flashcardId) = await SeedBoxWithFlashcardAsync();

        var today = StudySessionClock.LocalToday(DateTime.UtcNow);
        var session = StudySession.ForAdd(UserId, StudySessionScopeType.Box, boxId, today);
        session.ShowFlashcard(flashcardId);
        await _sessionRepository.AddAsync(session, _cancellationToken);
        await _unitOfWork.CommitAsync(_cancellationToken);

        await _commandDispatcher.Dispatch(new ShowLaterProcessCommand(flashcardId, null, UserId), _cancellationToken);

        var reloaded = await _sessionRepository.GetAsync(UserId, StudySessionScopeType.Box, boxId, today, _cancellationToken);

        reloaded.ShouldNotBeNull();
        reloaded!.CurrentFlashcardId.ShouldBeNull();
        reloaded.Cards.Count.ShouldEqual(1);
        reloaded.Cards[0].FlashcardId.ShouldEqual(flashcardId);
        reloaded.Cards[0].Status.ShouldEqual(StudySessionCardStatus.Deferred);
    }

    [Fact]
    public async Task Handle_Throws_WhenFlashcardIsNotTheCurrentOneShown()
    {
        var (boxId, flashcardId) = await SeedBoxWithFlashcardAsync();

        var otherFlashcard = Flashcard.ForAdd(boxId, 2, "front2", "back2", "front2", "back2", ["tag"]);
        await _flashcardRepository.AddAsync(otherFlashcard);
        await _unitOfWork.CommitAsync(_cancellationToken);

        var today = StudySessionClock.LocalToday(DateTime.UtcNow);
        var session = StudySession.ForAdd(UserId, StudySessionScopeType.Box, boxId, today);
        session.ShowFlashcard(flashcardId); // کارت اول الان نمایش داده می‌شود، نه otherFlashcard
        await _sessionRepository.AddAsync(session, _cancellationToken);
        await _unitOfWork.CommitAsync(_cancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _commandDispatcher.Dispatch(new ShowLaterProcessCommand(otherFlashcard.Id, null, UserId), _cancellationToken));
    }
}
