using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Commands;

/// <param name="Amount">همان عددی که پایان مرور پیشنهاد داد؛ حداکثر ۱۰.</param>
public record AddBacklogBonusCommand(long BoxId, int Amount) : ICommand;

/// <summary>«بیشتر می‌خوانم» در پایان مرور: چند کارت عقب‌افتادهٔ دیگر برای امروز باز می‌شود (<see cref="Box.AddBacklogBonus"/>).</summary>
public class AddBacklogBonusCommandHandler(IBoxRepository boxes, IUnitOfWork unitOfWork) : ICommandHandler<AddBacklogBonusCommand>
{
    private const int MaxPerRequest = 10;

    public async Task Handle(AddBacklogBonusCommand command, CancellationToken cancellationToken)
    {
        var box = await boxes.GetByIdAsync(command.BoxId, cancellationToken) ?? throw new BoxNotFoundException(command.BoxId);

        box.AddBacklogBonus(StudySessionClock.LocalToday(DateTime.UtcNow), Math.Clamp(command.Amount, 1, MaxPerRequest));

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
