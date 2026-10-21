using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public abstract class ReadService
{
    protected readonly ReadModelDbContext Context;

    protected DateTime Today => DateTime.Today;
    
    protected ReadService(ReadModelDbContext context)
    {
        Context = context;
    }
}