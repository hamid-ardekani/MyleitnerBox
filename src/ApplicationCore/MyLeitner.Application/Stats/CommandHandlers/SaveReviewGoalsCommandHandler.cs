using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.ReviewGoals;

namespace MyLeitner.Application.Stats.CommandHandlers;

public class SaveReviewGoalsCommandHandler(IReviewGoalRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<SaveReviewGoalsCommand>
{
    public async Task Handle(SaveReviewGoalsCommand command, CancellationToken cancellationToken)
    {
        var goal = await repository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (goal == null)
            await repository.AddAsync(ReviewGoal.ForUser(command.UserId, command.Daily, command.Weekly, command.Monthly), cancellationToken);
        else
            goal.Update(command.Daily, command.Weekly, command.Monthly);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
