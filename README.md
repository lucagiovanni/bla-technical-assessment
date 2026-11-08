# Task Manager

## User story

As a person with a personal task list, I can register, log in, and create, read, update, and delete my own tasks. Each task has a title, a description, a status (`Pending`, `InProgress`, or `Completed`), and a due date. A task belongs to the person who created it. People only see and change their own tasks.

## Architecture

The Angular client in `client/` talks to one ASP.NET Core host.

- The client calls `/api`. In development, `client/proxy.conf.json` forwards that prefix to `http://localhost:5080`.
- `src/TaskManager.Api` is the host. MVC controllers expose two route groups: `/api/users` (register, login, and the authorized current user) and `/api/tasks` (create, read, update, and delete). JWT is required for `/api/users/me` and for every task route. In Development, CORS allows `http://localhost:4200`.
- `src/TaskManager.Application` owns validation, ownership, registration, and login. It does not reference the database or HTTP. Use cases return an outcome, and the API maps that outcome to a status code.
- `src/TaskManager.Domain` holds `User`, `TaskItem`, and `TaskState`.
- `src/TaskManager.Infrastructure` implements the application ports with EF Core and SQLite, and hashes passwords with PBKDF2.

A task that belongs to someone else is returned as not found. Each project under `src/` has a matching test project under `tests/`.

## Demo credentials

- Email: `demo@taskmanager.local`
- Password: `Demo123!`

On startup the API creates a local SQLite file named `taskmanager.db` when it is missing, and seeds that user with three tasks. The database file is gitignored. The JWT signing key in `src/TaskManager.Api/appsettings.json` is for this demo only.

## Run the API

From the repository root:

```bash
dotnet build
dotnet test
dotnet run --project src/TaskManager.Api
```

The host listens on `http://localhost:5080`.

## Run the Angular client

From `client/`, with the API already running:

```bash
npm ci
npm start
npm test
```

`npm start` serves the client at `http://localhost:4200`. `npm test` runs the unit tests once and then exits.

## License

MIT. See [LICENSE](LICENSE).
