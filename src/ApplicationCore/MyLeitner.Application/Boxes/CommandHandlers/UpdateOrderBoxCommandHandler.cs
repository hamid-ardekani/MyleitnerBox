using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Services;
using MyLeitner.Common;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;

namespace MyLeitner.Application.Boxes.CommandHandlers;

public class UpdateOrderBoxCommandHandler : ICommandHandler<UpdateOrderBoxCommand>
{
    private readonly IBoxRepository _boxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderBoxCommandHandler(IBoxRepository boxRepository, IUnitOfWork unitOfWork)
    {
        _boxRepository = boxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateOrderBoxCommand command, CancellationToken cancellationToken)
    {
        var existingBox = await _boxRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingBox == null)
            throw new BoxNotFoundException(command.Id);

        var boxes = await _boxRepository.GetAllBoxesByUserIdAsync(existingBox.UserId, cancellationToken);

        var items = boxes
            .Select(c => new OrderItem(c.Id, c.OrderBox)) 
            .ToList();
        ChangeOrderManager.ChangeOrderInPlace(items, existingBox.Id, command.Order);

        foreach (var box in boxes)
            box.UpdateOrder(items.First(c => c.Id == box.Id).Order);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}