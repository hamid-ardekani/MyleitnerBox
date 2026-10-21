using Dela.Toolkit.Application;

namespace MyLeitner.Application.Contract.Commands;

public record AddCategoryCommand(string Name, List<long> BoxIds, long UserId) : ICommand;

public record UpdateCategoryCommand(long Id, string Name, List<long> BoxIds, long UserId) : ICommand;

public record CategoryBoxCommand(short Order, long BoxId) : ICommand;

public record UpdateOrderCategoryCommand(long Id, short Order, long UserId) : ICommand;

public record DeleteCategoryCommand(long Id) : ICommand;