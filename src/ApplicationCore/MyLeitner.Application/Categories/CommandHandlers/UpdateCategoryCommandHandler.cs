using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Categories;
using MyLeitner.Domain.Categories.Exceptions;
using MyLeitner.Domain.Users;

namespace MyLeitner.Application.Categories.CommandHandlers;

public class UpdateCategoryCommandHandler: ICommandHandler<UpdateCategoryCommand>
{
    private readonly IUserDomainService _userDomainService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IUserDomainService userDomainService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _userDomainService = userDomainService;
    }

    public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoriesUserAddedCount = await _categoryRepository.GetCategoryByUserIdCountAsync(command.UserId);

        if (!await _userDomainService.CanUserAddCategory(command.UserId, categoriesUserAddedCount))
            return;
        
        if (!await _userDomainService.CanUserAddBoxIntoCategory(command.UserId,command.BoxIds))
            return;
        
        var existingCategory = await _categoryRepository.GetByIdAsync(command.Id,cancellationToken);
        if(existingCategory == null)
            throw new CategoryNotFoundException(command.Id);
  
        existingCategory.Update(command.Name, command.BoxIds);

        var result = await _unitOfWork.CommitAsync(cancellationToken);
    }
}