namespace MyLeitner.Application.Contract.EventDispatcher;

public interface IDomainEventHandler<T>
{
    Task Handle(T domainEvent, CancellationToken ct);
}