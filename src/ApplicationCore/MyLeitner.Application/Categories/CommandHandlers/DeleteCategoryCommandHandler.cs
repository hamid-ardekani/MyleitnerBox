using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Categories;
using MyLeitner.Domain.Categories.Exceptions;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Categories.CommandHandlers;

public class DeleteCategoryCommandHandler: ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IStudySessionRepository studySessionRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _studySessionRepository = studySessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingCategory == null)
            throw new CategoryNotFoundException(command.Id);

        _categoryRepository.Delete(existingCategory);
        await _studySessionRepository.RemoveAllForScopeAsync(StudySessionScopeType.Category, command.Id, cancellationToken);

        var categories = await _categoryRepository.GetAllCategoriesByUserIdAsync(existingCategory.UserId, cancellationToken);
        short order = 1;
        foreach (var category in categories)
        {
            if (category.Id == command.Id)
                continue;

            category.UpdateOrder(order++);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}