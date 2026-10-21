using Dela.Toolkit.Application;

namespace MyLeitner.Application.Contract.Commands;

public record AddBoxCommand(string Name, bool ReadAloud, long UserId, short? DailyNewCardLimit = null, short? DailyBacklogFloor = null) : ICommand;

public record UpdateBoxCommand(long Id,string Name, bool ReadAloud, short? DailyNewCardLimit = null, short? DailyBacklogFloor = null) : ICommand;

public record UpdateOrderBoxCommand(long Id,short Order, long UserId) : ICommand;

public record DeleteBoxCommand(long Id) : ICommand;