using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Services;
using MyLeitner.Common;
using MyLeitner.Domain.Categories;
using MyLeitner.Domain.Categories.Exceptions;

namespace MyLeitner.Application.Categories.CommandHandlers;

public class UpdateOrderCategoryCommandHandler: ICommandHandler<UpdateOrderCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateOrderCategoryCommand command, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingCategory == null)
            throw new CategoryNotFoundException(command.Id);

        var categories = await _categoryRepository.GetAllCategoriesByUserIdAsync(existingCategory.UserId, cancellationToken);

        var items = categories
            .Select(c => new OrderItem(c.Id, c.Order)) 
            .ToList();
        
        ChangeOrderManager.ChangeOrderInPlace(items, existingCategory.Id, command.Order);

        foreach (var category in categories)
            category.UpdateOrder(items.First(c => c.Id == category.Id).Order);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}