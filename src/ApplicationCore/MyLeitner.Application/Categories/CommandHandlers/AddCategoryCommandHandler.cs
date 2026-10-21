using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Categories;
using MyLeitner.Domain.Users;

namespace MyLeitner.Application.Categories.CommandHandlers;

 
public class AddCategoryCommandHandler : ICommandHandler<AddCategoryCommand>
{
    private readonly IUserDomainService _userDomainService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IUserDomainService userDomainService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _userDomainService = userDomainService;
    }

    public async Task Handle(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoriesUserAddedCount = await _categoryRepository.GetCategoryByUserIdCountAsync(command.UserId);

        if (!await _userDomainService.CanUserAddCategory(command.UserId, categoriesUserAddedCount))
            return;
 
        var order = (short)(categoriesUserAddedCount + 1);
 
        var category = Category.CategoryForAdd(command.Name, order, command.UserId);
        
        foreach (var boxId in command.BoxIds)
            category.AddBox(boxId);
        
        await _categoryRepository.AddAsync(category);

        var result = await _unitOfWork.CommitAsync(cancellationToken);
    }
}