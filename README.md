# UserManagementAPI — TechHive Solutions

A complete repository-ready **ASP.NET Core (.NET 10) Minimal Web API** sample for managing internal user records. Includes CRUD endpoints, validation, appropriate HTTP errors, centralized exception handling, request logging, optional API-key middleware, a Postman collection, dependency-free smoke tests, and a GitHub Actions workflow.

> This is an educational sample with **in-memory storage**; user data is lost on application restart. Use a persistent database and enterprise authentication/authorization before deploying with real personal data. Do not store sensitive HR data in a publicly accessible demo.

## 1. Run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). From the repository root:

```bash
dotnet restore UserManagementAPI.slnx
dotnet build UserManagementAPI.slnx
dotnet run --project UserManagementAPI/UserManagementAPI.csproj
```

The launch profile listens at `http://localhost:5100`.

Check readiness:

```bash
curl http://localhost:5100/health
```

### Optional API-key authentication

All `/api/users` endpoints can be protected with a user-defined API key. Leave the setting unset for **local coursework testing only**. Set your own long random secret before exposing the API; the `/health` endpoint stays public. The key is never included in this repository.

**PowerShell:**

```powershell
$env:ApiSecurity__ApiKey = "choose-a-long-random-secret"
dotnet run --project UserManagementAPI/UserManagementAPI.csproj
```

**Bash:**

```bash
export ApiSecurity__ApiKey='choose-a-long-random-secret'
dotnet run --project UserManagementAPI/UserManagementAPI.csproj
```

When enabled, clients must send `X-API-Key: <your-key>`. API keys are for a teaching example, not a replacement for production-grade identity and role-based authorization.

## 2. API endpoints

| Verb | Path | Description | Success | Failure examples |
|------|------|-------------|---------|------------------|
| `GET` | `/health` | Health check | 200 | — |
| `GET` | `/api/users` | List users | 200 | 401 if key is required |
| `GET` | `/api/users/{id}` | Get user by GUID | 200 | 404 |
| `POST` | `/api/users` | Create a user | 201 + Location | 400, 409 |
| `PUT` | `/api/users/{id}` | Replace user details | 200 | 400, 404, 409 |
| `DELETE` | `/api/users/{id}` | Remove a user | 204 | 404 |

Example request:

```http
POST /api/users HTTP/1.1
Host: localhost:5100
Content-Type: application/json

{
  "name": "Sarah Ahmed",
  "email": "sarah@example.com",
  "department": "HR"
}
```

Example response: `201 Created`, a `Location: /api/users/<guid>` response header and a JSON user object containing `id`, `name`, `email`, `department`, `createdAtUtc`, and `updatedAtUtc`.

Validation enforces required, non-whitespace `name` (max 100), `email` (max 254 and valid format), and `department` (max 80). Surrounding whitespace is removed. Email addresses must be unique **case-insensitively**. Invalid data yields a `400` validation problem JSON; duplicates return `409` and unknown valid GUIDs return `404`. Unhandled exceptions return a sanitized `500` problem response with a trace ID.

## 3. Test the API

Start the API in one terminal (see above), then in another terminal run:

```bash
python3 tests/smoke_test.py --base-url http://localhost:5100
```

Or, if you set the API key:

```bash
python3 tests/smoke_test.py --base-url http://localhost:5100 --api-key 'choose-a-long-random-secret'
```

These tests check health, listing, create/retrieve/update/delete, invalid input, missing fields, duplicate-email conflicts, not-found errors, and authentication when enabled. They create unique test records and remove them.

For manual testing, import `postman/UserManagementAPI.postman_collection.json` into Postman. If you enabled the API key, set the collection variable `apiKey` to your configured secret; otherwise leave it empty. Execute **Create User** first; its test script stores the returned ID in the collection variable `userId`. You can also use `UserManagementAPI/UserManagementAPI.http` in VS Code with the REST Client extension or supported IDEs.

For continuous integration, `.github/workflows/ci.yml` builds the API on each push and pull request, launches it with API-key protection enabled, and executes the Python smoke test. Check the **Actions** tab after pushing to GitHub.

## 4. Project structure

```text
UserManagementAPI/
├── UserManagementAPI/
│   ├── Endpoints/UserEndpoints.cs       CRUD endpoints and HTTP status codes
│   ├── Models/                         User and request DTOs
│   ├── Repositories/                   Thread-safe in-memory repository
│   ├── Validation/UserValidator.cs     Incoming-field validation
│   ├── Middleware/                     Logging, error handling, optional API key
│   ├── Program.cs                      Service setup and middleware pipeline
│   └── UserManagementAPI.csproj
├── tests/smoke_test.py                 CRUD and edge-case tests
├── postman/                            Postman collection
├── docs/Copilot-Debugging-Worksheet.md Copilot prompts and evidence worksheet
├── UserManagementAPI.slnx              Open/build solution from repository root
├── .github/workflows/ci.yml           Automated GitHub build/test
└── README.md
```

## 5. Grading rubric (25 points)

| Criterion | Implementation and evidence | Status |
|-----------|-----------------------------|--------|
| Public GitHub repository (5) | Follow the steps below, then submit its link | **Requires your GitHub upload** |
| CRUD endpoints (5) | `Endpoints/UserEndpoints.cs` + tests | Implemented in source |
| Copilot debugging (5) | `docs/Copilot-Debugging-Worksheet.md` | **Requires actual Copilot session and documentation** |
| Validation (5) | `Validation/UserValidator.cs`; invalid input tests | Implemented in source |
| Middleware (5) | `Middleware/RequestLoggingMiddleware.cs`, `ExceptionHandlingMiddleware.cs`, `ApiKeyMiddleware.cs` | Implemented in source |

A repository being packaged here does **not** mean it was published to your GitHub account, and this project does **not** pretend Copilot was used. Those two actions require you to complete them honestly.

## 6. Publish to a public GitHub repository

1. Sign in to [GitHub](https://github.com/) and create a **public** repository named `UserManagementAPI` (leave GitHub's "Add a README" unchecked to avoid a conflicting initial commit).
2. Download and extract this archive. Open a terminal in the **outer** `UserManagementAPI` folder (the one containing this README and `.gitignore`).
3. Run the commands below, replacing `YOUR_USERNAME`:

```bash
git init
git add .
git commit -m "Initial commit: User Management API with CRUD and middleware"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/UserManagementAPI.git
git push -u origin main
```

4. Wait for GitHub Actions to finish and inspect any build/test failures. Fix any issues, commit and push again.
5. Actually use Copilot to inspect/debug code and complete the worksheet with real evidence.
6. Submit the final GitHub URL: `https://github.com/YOUR_USERNAME/UserManagementAPI`.

Do not upload `.env` or real API keys. GitHub authentication during `git push` uses a browser/credential manager or token, **not** your GitHub password.

## 7. Debugging notes

The design addresses the bugs described by TechHive: endpoints validate required fields and emails before saving; missing users return 404 instead of crashing; unexpected exceptions are captured by error-handling middleware; and a lock plus indexed email lookup prevents racing duplicate inserts and expensive duplicate-email scans. These are **design decisions of this project**. To claim Copilot assistance on a graded submission, follow and document the real Copilot workflow in `docs/Copilot-Debugging-Worksheet.md`.
