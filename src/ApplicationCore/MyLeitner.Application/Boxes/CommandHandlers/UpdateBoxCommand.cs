using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions; 

namespace MyLeitner.Application.Boxes.CommandHandlers;

public class UpdateBoxCommandHandler : ICommandHandler<UpdateBoxCommand>
{
    private readonly IBoxRepository _boxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBoxCommandHandler(IBoxRepository boxRepository, IUnitOfWork unitOfWork)
    {
        _boxRepository = boxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateBoxCommand command, CancellationToken cancellationToken)
    { 
        var existingBox = await _boxRepository.GetByIdAsync(command.Id,cancellationToken);
        if(existingBox == null)
            throw new BoxNotFoundException(command.Id);
      
        existingBox.Update(command.Name, command.ReadAloud, command.DailyNewCardLimit, command.DailyBacklogFloor);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}