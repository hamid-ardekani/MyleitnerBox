using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLeitner.Application.Boxes.CommandHandlers;
using MyLeitner.Application.Boxes.Queries;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Categories.CommandHandlers;
using MyLeitner.Application.Categories.Queries;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.EventDispatcher;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Application.DomainEventHandlers;
using MyLeitner.Application.Flashcards;
using MyLeitner.Application.Flashcards.CommandHandlers;
using MyLeitner.Application.Flashcards.Queries;
using MyLeitner.Application.Flashcards.Queries.Models;
using MyLeitner.Application.Stats.CommandHandlers;
using MyLeitner.Application.Stats.Queries;
using MyLeitner.Application.Stats.Queries.Models;
using MyLeitner.Config.Services;
using MyLeitner.Domain.ReviewGoals;
using MyLeitner.Domain.ReviewLogs;
using MyLeitner.Application.Learnings.Commands;
using MyLeitner.Application.Learnings.Queries;
using MyLeitner.Common.Results;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Categories;
using MyLeitner.Domain.Common;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Events;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Domain.Users;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.Service;
using MyLeitner.Persistence.WriteModel;
using MyLeitner.Persistence.WriteModel.Repositories;

namespace MyLeitner.Config;

/// <summary>تنها جای ثبت وابستگی‌ها؛ هم API واقعی (<c>Program.cs</c>) و هم زیرساخت تست (<see cref="MyLeitnerModule"/> روی Autofac) از همین متد استفاده می‌کنند.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configureWriteDb = null,
        Action<DbContextOptionsBuilder>? configureReadDb = null)
    {
        //Domain
        services.AddTransient<IUserDomainService, UserDomainService>();

        //Events
        services.AddScoped<IDomainEventHandler<FlashcardCreatedEvent>, CreateLearningScheduleOnFlashcardCreated>();

        //Application

        // Queries
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddScoped<IQueryHandler<GetBoxesQuery, List<BoxListItemDto>>, GetBoxesQueryHandler>();
        services.AddScoped<IQueryHandler<GetBoxQuery, BoxForEditDto?>, GetBoxQueryHandler>();

        services.AddScoped<IQueryHandler<GetFlashcardQuery, FlashcardForSaveDto>, GetFlashcardQueryHandler>();
        services.AddScoped<IQueryHandler<GetFlashcardsQuery, List<FlashcardListItemDto>>, GetFlashcardsQueryHandler>();
        services.AddScoped<IQueryHandler<SearchFlashcardsQuery, FlashcardSearchResultDto>, SearchFlashcardsQueryHandler>();
        services.AddScoped<IQueryHandler<FindSimilarFlashcardsQuery, List<SimilarFlashcardDto>>, FindSimilarFlashcardsQueryHandler>();

        services.AddScoped<IQueryHandler<GetCategoriesQuery, List<CategoryListItemDto>>, GetCategoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCategoryQuery, CategoryForEditDto?>, GetCategoryQueryHandler>();

        services.AddScoped<IQueryHandler<GetNextCardInCategoryQuery, CardDto>, GetNextCardInCategoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetNextPlayFlashcardInBoxQuery, CardDto>, GetNextPlayFlashcardInBoxQueryHandler>();
        services.AddScoped<IQueryHandler<GetNextPlayFlashcardInCategoryQuery, Result<StudySessionResult>>, GetNextPlayFlashcardInCategoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetAnswerPreviewQuery, List<AnswerIntervalPreviewDto>>, GetAnswerPreviewQueryHandler>();
        services.AddScoped<IQueryHandler<GetStatsQuery, StatsDto>, GetStatsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecapQuery, RecapDto>, GetRecapQueryHandler>();
        services.AddScoped<IQueryHandler<GetProgressQuery, ProgressDto>, GetProgressQueryHandler>();
        services.AddScoped<IQueryHandler<GetHomeQuery, HomeDto>, GetHomeQueryHandler>();
        services.AddScoped<IQueryHandler<GetStudyOverviewQuery, StudyOverviewDto>, GetStudyOverviewQueryHandler>();
        services.AddScoped<IQueryHandler<GetPracticeQuery, PracticeDto>, GetPracticeQueryHandler>();
        services.AddScoped<IQueryHandler<GetPracticeCountsQuery, PracticeCountsDto>, GetPracticeCountsQueryHandler>();

        // Commands
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<ICommandHandler<AddCategoryCommand>, AddCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCategoryCommand>, UpdateCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOrderCategoryCommand>, UpdateOrderCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCategoryCommand>, DeleteCategoryCommandHandler>();

        services.AddScoped<ICommandHandler<AddBoxCommand>, AddBoxCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBoxCommand>, UpdateBoxCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOrderBoxCommand>, UpdateOrderBoxCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteBoxCommand>, DeleteBoxCommandHandler>();

        var uploadsRootPath = ImageStoragePaths.ResolveUploadsRootPath(configuration);
        services.AddScoped<IImageStorageService>(_ => new LocalImageStorageService(uploadsRootPath));
        services.AddScoped<IFlashcardContentImageResolver, FlashcardContentImageResolver>();
        services.AddScoped<ICommandHandler<AddFlashcardCommand>, AddFlashcardCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateFlashcardCommand>, UpdateFlashcardCommandHandler>();
        services.AddScoped<ICommandHandler<MoveFlashcardOrderUpCommand>, MoveFlashcardOrderUpCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteFlashcardCommand>, DeleteFlashcardCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveFlashcardCommand>, ArchiveFlashcardCommandHandler>();
        services.AddScoped<ICommandHandler<RestoreFlashcardCommand>, RestoreFlashcardCommandHandler>();
        services.AddScoped<ICommandHandler<AddFlashcardToReviewCommand>, AddFlashcardToReviewCommandHandler>();

        services.AddScoped<ICommandHandler<AnswerProcessCommand>, AnswerProcessCommandHandler>();
        services.AddScoped<ICommandHandler<ShowLaterProcessCommand>, ShowLaterProcessCommandHandler>();
        services.AddScoped<ICommandHandler<MarkKnownCommand>, MarkKnownCommandHandler>();
        services.AddScoped<ICommandHandler<AddBacklogBonusCommand>, AddBacklogBonusCommandHandler>();
        services.AddScoped<ICommandHandler<SaveReviewGoalsCommand>, SaveReviewGoalsCommandHandler>();

        services.AddWriteModel(configuration, configureWriteDb);
        services.AddReadModel(configuration, configureReadDb);


        return services;
    }

    private static void AddWriteModel(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder>? configureDb)
    {
        services.AddDbContext<MyLeitnerDbContext>(options =>
        {
            if (configureDb != null)
            {
                configureDb(options);
                return;
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            options.UseSqlServer(
                connectionString,
                sqlServerOptions => { sqlServerOptions.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds); });
        });

        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
        services.AddScoped<ITransactionalUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
        services.AddScoped<IBoxRepository, BoxRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IFlashcardRepository, FlashcardRepository>();
        services.AddScoped<ILearningScheduleRepository, LearningScheduleRepository>();
        services.AddScoped<IStudySessionRepository, StudySessionRepository>();
        services.AddScoped<IReviewLogRepository, ReviewLogRepository>();
        services.AddScoped<IReviewGoalRepository, ReviewGoalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    private static void AddReadModel(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder>? configureDb)
    {
        services.AddDbContext<ReadModelDbContext>(options =>
        {
            if (configureDb != null)
                configureDb(options);
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => { sqlServerOptions.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds); });
            }

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<ICategoryReadService, CategoryReadService>();
        services.AddScoped<IBoxReadService, BoxReadService>();
        services.AddScoped<IFlashcardReadService, FlashcardReadService>();
        services.AddScoped<IStatsReadService, StatsReadService>();
        services.AddScoped<ICardReadRepository, CardReadRepository>();
        services.AddScoped<IOwnershipService, OwnershipService>();
    }
}
