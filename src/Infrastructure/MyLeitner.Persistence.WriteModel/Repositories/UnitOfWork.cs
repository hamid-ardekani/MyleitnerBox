using Dela.Toolkit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyLeitner.Application.Contract.EventDispatcher;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Domain.Common;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class UnitOfWork : ITransactionalUnitOfWork
{
    private readonly MyLeitnerDbContext _context;
    private readonly IServiceProvider _provider;

    public UnitOfWork(MyLeitnerDbContext context, IServiceProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DomainEventDispatchingInProcess(  cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception e)
        {
            if (e.InnerException != null &&
                (e.InnerException.Message.Contains("Cannot insert duplicate key row in object 'dbo.Boxes' with unique index 'IX_Boxes_UserId_Name'.")
                 || e.InnerException.Message.Contains("Cannot insert the value NULL into column")))
                throw new DuplicateBoxException();


            throw;
        }
    }

    public Task<int> ExecuteSqlRawAsync(string commandText)
    {
        throw new NotImplementedException();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        // provider درون‌حافظه (در تست‌ها) تراکنش را پشتیبانی نمی‌کند
        if (!_context.Database.IsRelational())
        {
            await action();
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // رویدادهای دامنه (درون‌حافظه / درون‌فرایند)
    private async Task DomainEventDispatchingInProcess(CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = _context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IAggregateRoot>()
            .Where(a => a.UncommittedEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var @event in aggregate.UncommittedEvents.ToList())
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
                var handlers = _provider.GetServices(handlerType);

                foreach (dynamic handler in handlers)
                    await handler.Handle((dynamic)@event, cancellationToken);
            }

            // جلوگیری از ارسال دوبارهٔ یک رویداد در CommitAsync بعدی، وقتی scope/DbContext بیش از یک دستور عمر می‌کند
            // (مثلاً تست‌هایی که چند دستور را روی یک scope مشترک اجرا می‌کنند)
            aggregate.ClearEvents();
        }
    }
}