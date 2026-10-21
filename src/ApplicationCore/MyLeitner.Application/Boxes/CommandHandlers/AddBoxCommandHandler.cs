using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Users;

namespace MyLeitner.Application.Boxes.CommandHandlers;

public class AddBoxCommandHandler : ICommandHandler<AddBoxCommand>
{
    private readonly IUserDomainService _userDomainService;
    private readonly IBoxRepository _boxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBoxCommandHandler(IBoxRepository boxRepository, IUnitOfWork unitOfWork, IUserDomainService userDomainService)
    {
        _boxRepository = boxRepository;
        _unitOfWork = unitOfWork;
        _userDomainService = userDomainService;
    }

    public async Task Handle(AddBoxCommand command, CancellationToken cancellationToken)
    {
        var boxesUserAddedCount = await _boxRepository.GetBoxByUserIdCountAsync(command.UserId);

        if (!await _userDomainService.CanUserAddBox(command.UserId, boxesUserAddedCount))
            return;
 
        var order = (short)(boxesUserAddedCount + 1);
 
        var box = Box.BoxForAdd(command.Name, order, command.ReadAloud, command.UserId, command.DailyNewCardLimit, command.DailyBacklogFloor);
        
        await _boxRepository.AddAsync(box);

        var result = await _unitOfWork.CommitAsync(cancellationToken);
    }
}