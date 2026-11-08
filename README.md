# Task Manager

A personal task list. A person registers, signs in, and manages their own tasks. The server is one ASP.NET Core host split into Clean Architecture projects:

- `src/TaskManager.Domain`
- `src/TaskManager.Application`
- `src/TaskManager.Infrastructure`
- `src/TaskManager.Api`

Each project has a matching test project under `tests/`.

## API

From the repository root:

```bash
dotnet build
dotnet test
dotnet run --project src/TaskManager.Api
```

The host listens on `http://localhost:5080`.
