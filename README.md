# MyLeitner API

Backend for **MyLeitner**, a flashcard app built on the Leitner box method with spaced repetition. Users organize cards into boxes and categories, review the cards due each day, and track their progress.

## Tech stack

- ASP.NET Core, .NET 10
- EF Core, SQL Server
- Clean / Onion architecture, CQRS, DDD
- JWT authentication with rotating refresh tokens
- Built on the in-house `Dela.Toolkit` framework (not included in this repository)

## Project structure

```
src/
  Api/                MyLeitner.Api                  Controllers, auth, rate limiting
  ApplicationCore/
    MyLeitner.Domain                                 Aggregates and domain rules
    MyLeitner.Application                            Command/query handlers
    MyLeitner.Application.Contract                   DTOs, commands, interfaces
    MyLeitner.Common                                 Result types and shared helpers
  Infrastructure/
    MyLeitner.Persistence.WriteModel                 EF Core write model and migrations
    MyLeitner.Persistence.ReadModel.Sql              No-tracking read model and queries
    MyLeitner.Identity                               Registration, login, tokens
    MyLeitner.Config                                 Dependency injection setup
test/                                                xUnit tests
tools/                                                One-off legacy data migrator
```

## Key features

- Per-card long-term scheduling (stability, difficulty, next review date)
- Daily study sessions with next-card selection
- Daily new-card quota and overdue ("backlog") quota per box
- Full-text style search and similar-card detection
- Review goals, progress stats and streaks

## Getting started

1. Set the connection string (`ConnectionStrings:DefaultConnection`), `Jwt:Key` (at least 32 characters) and `Registration:InviteCode` in `appsettings.Development.json` or user secrets.
2. Apply the database migrations (see below).
3. Run the API:

```bash
dotnet run --project src/Api/MyLeitner.Api
```

Swagger is available at `/swagger`.

## Database migrations

Run from `src/Infrastructure/MyLeitner.Persistence.WriteModel`:

```bash
# add a migration
dotnet ef migrations add <Name> --context MyLeitner.Persistence.WriteModel.MyLeitnerDbContext --startup-project ..\..\Api\MyLeitner.Api\MyLeitner.Api.csproj

# update the database
dotnet ef database update --context MyLeitner.Persistence.WriteModel.MyLeitnerDbContext --startup-project ..\..\Api\MyLeitner.Api\MyLeitner.Api.csproj
```

## Tests

```bash
dotnet test
```

Some tests use a SQL Server database named `MyLeitnerTest` (see `test/CommandHandler_Test/Application.cs`) and need access to a local instance.
