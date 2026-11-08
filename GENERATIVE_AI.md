# Generative AI

The implementation in this repository was produced in Cursor by Grok 4.7. It was not a separate chat that only scaffolded an API. The assessment request below was the prompt, and the task-API section of the exercise was applied inside that request. This file quotes that request and the code that was committed. It does not invent a second conversation.

## Assessment request

Build the Ballast Lane technical assessment and push it to the existing public repository https://github.com/lucagiovanni/bla-technical-assessment. Keep the existing MIT `LICENSE` and the initial commit `c9cf0a6` ("Initial commit"). Do not force-push. Replace the placeholder `README.md` in the first planned commit.

Follow the exercise email, the exercise PDF, and the 15-commit plan. Where they disagree, follow the stricter instruction. The client is Angular. Where the PDF names Vue, use Angular and keep the behavior the PDF describes. The PDF does not name a database, so use SQLite. Use one ASP.NET Core host with two API route groups, users and tasks.

Implement the 15 commits in order. Each commit is one reviewable change, uses the planned imperative sentence, leaves the tree buildable, and includes the tests for the behavior it adds. Push each commit to `origin main` before the next substantial slice. After each server commit, `dotnet build` and `dotnet test` must pass. After each client commit, the client must build and `npm test` must pass.

Do not commit the exercise email, the PDF, the commit plan, secrets beyond the named demo user, local databases, machine-specific paths, or symbolic links. Ignore `bin/`, `obj/`, `node_modules/`, `dist/`, and `.angular/`.

Before calling the work done, run the app and the tests the way the README describes and fix failures. This file must record the real prompt, the code that was actually committed, how that result was checked, and what was corrected.

## Task-API prompt that was applied

This is the generative-AI section of the exercise, applied while building the task API:

> Imagine you're tasked with generating a RESTful API for a simple task management system using your preferred language. The system should support the following functionality:
>
> - Create, read, update, and delete tasks (CRUD)
> - Each task has a title, description, status, and due_date
> - Tasks are associated with a user (assume basic User model exists)
>
> Using your preferred GenAI coding tool, write the prompt you would use to generate the API scaffold or full implementation. Show the output code (or a representative sample of it). Describe how you validated the AI's suggestions, corrected or improved the output if necessary, and handled edge cases, authentication, or validations.

The language used was C#. The user model and the task model were added first, in the domain project, and the task use cases were added against repository ports. The HTTP layer came after those rules had tests. The PDF's `due_date` is `dueDate` in JSON and `DueDate` in C#. Status values are `Pending`, `InProgress`, and `Completed`.

## Representative result

This is the committed create use case, `src/TaskManager.Application/Tasks/CreateTask.cs`. The same validation path is used by update.

```csharp
public async Task<Outcome<TaskModel>> ExecuteAsync(
    Guid ownerId,
    string? title,
    string? description,
    string? status,
    DateOnly? dueDate,
    CancellationToken cancellationToken)
{
    if (ownerId == Guid.Empty)
    {
        return Outcome<TaskModel>.Fail(Failure.Validation("Owner is required."));
    }

    var parsed = TaskRules.Parse(title, description, status, dueDate);
    if (parsed.Error is not null || parsed.Value is null)
    {
        return Outcome<TaskModel>.Fail(parsed.Error ?? Failure.Validation("Task is not valid."));
    }

    var task = new TaskItem(
        Guid.NewGuid(),
        ownerId,
        parsed.Value.Title,
        parsed.Value.Description,
        parsed.Value.Status,
        parsed.Value.DueDate);

    await _tasks.AddAsync(task, cancellationToken);
    return Outcome<TaskModel>.Success(TaskRules.ToModel(task));
}
```

`TaskRules.Parse` rejects a blank title, a title over 200 characters, a description over 4000 characters, a missing due date, and a status that is not one of the `TaskState` names. Matching is case-insensitive. Values such as `Done`, `In Progress`, and `1` are rejected. An empty description is stored as an empty string. A due date in the past is allowed.

The committed controller, `src/TaskManager.Api/Controllers/TasksController.cs`, maps those outcomes onto HTTP. Every action requires a bearer token. The owner id comes from the `sub` claim, not from the body. Create returns `201` with `Location: /api/tasks/{id}`.

```csharp
[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskBody? body, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id(User) is not Guid ownerId)
        {
            return Unauthorized(new ApiMessage(GetCurrentUser.UnauthenticatedMessage));
        }

        var result = await _create.ExecuteAsync(ownerId, body?.Title, body?.Description, body?.Status, body?.DueDate, cancellationToken);
        if (result.Error is not null || result.Value is null)
        {
            return ApiResults.From(result.Error ?? Failure.Validation("Title is required."));
        }

        return Created($"/api/tasks/{result.Value.Id}", result.Value);
    }
}
```

Get, update, and delete use the same owner check. A task owned by someone else is `404` with `{"message":"Task was not found."}`. The users API is the other route group on the same host: `POST /api/users/register`, `POST /api/users/login`, and `GET /api/users/me`.

## How the result was checked

Each server commit was checked with `dotnet build` and `dotnet test` before it was pushed. Each client commit was checked with `npm test` and `npm run build` before it was pushed. The final check used the commands in the README.

From the repository root:

- `dotnet build` succeeded with 0 warnings and 0 errors.
- `dotnet test` passed: Domain 23, Application 38, Infrastructure 7, API 16.
- `dotnet run --project src/TaskManager.Api` listened on `http://localhost:5080` and seeded the demo user.

Against that host:

- `POST /api/users/login` with `demo@taskmanager.local` / `Demo123!` returned 200 and a token. `GET /api/users/me` returned 200.
- `GET /api/tasks` returned the three seeded tasks.
- Create, update, read, and delete of a new task returned 201, 200, 200, and 204. A later read of the deleted task returned 404.
- A wrong password returned 401. A task list without a token returned 401. A blank title and a status of `Done` returned 400.
- A newly registered user received an empty task list. Get, update, and delete of a seeded task with that user's token returned 404, and the owner could still read it.
- Registering `demo@taskmanager.local` again returned 409.
- A development CORS preflight from `http://localhost:4200` was allowed.

From `client/`:

- `npm ci` completed.
- `npm test` passed: 10 files, 32 tests.
- `npm start` served `http://localhost:4200/`.
- `GET /` returned the Task Manager page. `POST /api/users/login` through that dev server returned the demo user, so the proxy reached the API.
- Headless Edge rendered `/login` with the sign-in form, and `/tasks` redirected a signed-out visitor to that form.
- The page console logged Vite's dev-server connection and Angular's development-mode line. No application error was logged. `npm start` is the development server, so that development-mode line is expected there. The production build does not serve that mode.

The signed-in task form was not clicked in a browser. No browser automation beyond a headless document dump was available, and that dump cannot sign in before the app reads `sessionStorage`. The task screen is covered by `client/src/app/tasks/task-page.component.spec.ts` and `task.service.spec.ts`, and the same create, update, and delete calls were made against the API.

## What was corrected

- The PDF allows React or Vue. The client is Angular, with the same register, login, and task CRUD behavior.
- The PDF asks for a second API. Users and tasks are two route groups on one host, not a second process and not a Razor UI.
- The task type is `TaskItem`, so it does not clash with `System.Threading.Tasks.Task`.
- Another person's task is hidden with 404 rather than revealed with 403.
- Passwords are stored with PBKDF2. The demo signing key in `src/TaskManager.Api/appsettings.json` is for local development only.
- An unknown email and a wrong password share `Invalid email or password.` Empty email and password are validation failures, not authentication failures.
- Field rules live in the application layer. The controller maps an outcome to a status code and does not reimplement them.
- Status must be an enum name. Numeric strings are not accepted.
- A unique-email race is caught in `UserRepository.AddAsync`. The repository returns false, and registration becomes a 409.
- The installed Node can run Angular CLI 21 and cannot run Angular CLI 22. The client uses CLI 21.2.29. `npm` 10.9.2 failed to install the new client because of an arborist `edgesOut` error. The lockfile was produced with npm 11, and `npm ci` with npm 10.9.2 then succeeded. The generated client README, `.vscode`, Prettier, and the default welcome page were removed.
- The interceptor test flushed a 401 without an error subscriber, so Vitest reported an unhandled error after the assertions had passed. The test now handles that error.

## Edge cases, authentication, and validation

Registration trims and lowercases the email, limits it to 320 characters, and requires a password of 8 to 200 characters with a letter and a digit. The client also requires an email shape and a password of at least 8 characters. The server remains the authority for the letter and digit rule.

Login issues an HMAC-SHA256 JWT for the `TaskManager` issuer and audience. The token expires in 8 hours. Missing or invalid tokens are rejected with 401. The Angular interceptor adds the bearer token only to URLs that start with `/api`, and the session is kept in `sessionStorage`.

Task ownership is applied again when listing, so a repository result that contained another person's tasks would still be filtered. Lists are ordered by due date and then by title. Deleting a user cascades to that user's tasks. The demo seed does nothing if the demo user id or email is already stored.
