using Dela.Toolkit.Domain;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Users;
using MyLeitner.Persistence.WriteModel;
using MyLeitner.Persistence.WriteModel.Repositories;

namespace CommandHandler_Test;

public static class Application
{
    private static readonly string ConnectionString = "data source=.;database=MyLeitnerTest;Integrated Security=SSPI;persist security info=True;TrustServerCertificate=True;";

    public static IUserDomainService UserDomainService => new UserDomainService();
    
    public static IUnitOfWork UnitOfWork(MyLeitnerDbContext context) => new UnitOfWork(context,null);
    
    public static IBoxRepository BoxRepository(MyLeitnerDbContext context) => new BoxRepository(context);  

    public static void ClearAllTables()
    {
        ClearTable("CategoryBoxes");
        ClearTable("Boxes");
        ClearTable("Categories");
        ClearTable("Flashcards");
    }
    private static void ClearTable(string tableName)
    {
        var context = CreateContext();
        context.Database.ExecuteSqlRaw($"DELETE FROM [{tableName}];");
        context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ('[{tableName}]', RESEED, 0);");
    }

    public static MyLeitnerDbContext CreateContext() => new (new DbContextOptionsBuilder<MyLeitnerDbContext>()
        .UseSqlServer(ConnectionString)
        .Options);
}