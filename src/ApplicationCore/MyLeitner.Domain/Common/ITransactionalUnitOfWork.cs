using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.Common;

public interface ITransactionalUnitOfWork : IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken);
}
