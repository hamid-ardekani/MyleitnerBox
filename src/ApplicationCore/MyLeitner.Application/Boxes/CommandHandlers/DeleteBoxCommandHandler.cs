using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Boxes.CommandHandlers;

public class DeleteBoxCommandHandler : ICommandHandler<DeleteBoxCommand>
{
    private readonly IBoxRepository _boxRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoxCommandHandler(IBoxRepository boxRepository, IStudySessionRepository studySessionRepository, IUnitOfWork unitOfWork)
    {
        _boxRepository = boxRepository;
        _studySessionRepository = studySessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteBoxCommand command, CancellationToken cancellationToken)
    {
        var existingBox = await _boxRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingBox == null)
            throw new BoxNotFoundException(command.Id);

        _boxRepository.Remove(existingBox);
        await _studySessionRepository.RemoveAllForScopeAsync(StudySessionScopeType.Box, command.Id, cancellationToken);

        var boxes = await _boxRepository.GetAllBoxesByUserIdAsync(existingBox.UserId, cancellationToken);
        short order = 1;
        foreach (var box in boxes)
        {
            if (box.Id == command.Id)
                continue;

            box.UpdateOrder(order++);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}