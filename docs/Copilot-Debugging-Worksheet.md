# Microsoft Copilot debugging worksheet

> **Important:** This file contains suggestions and a blank evidence template, **not** a claim that Copilot was used. To earn the Copilot debugging points, open the project in an editor with GitHub Copilot / Microsoft Copilot, actually run the prompts below, review the responses, apply or reject suggestions, and record your own evidence.

## Suggested Copilot workflow

1. Open `UserManagementAPI/Endpoints/UserEndpoints.cs` and `Validation/UserValidator.cs`.
2. Prompt Copilot: **"Review these ASP.NET Core CRUD endpoints for missing input validation, null handling, incorrect status codes, and security or reliability issues. Identify bugs without rewriting everything."**
3. Prompt Copilot: **"Check the in-memory repository for duplicate-email race conditions and slow user lookups. Propose a thread-safe approach with tests."**
4. Prompt Copilot: **"Inspect the exception and request-logging middleware. Identify possible unhandled errors or accidental logging of user data."**
5. Prompt Copilot: **"Generate tests for invalid emails, empty names, duplicate emails, nonexistent user IDs, and DELETE followed by GET."**
6. Review Copilot's output carefully. Change code as appropriate and run `python3 tests/smoke_test.py` while the API is running.

## Record actual usage (fill in after using Copilot)

- Date / editor:
- Copilot prompt used:
- Issue Copilot identified:
- Copilot suggestion:
- Did I accept, modify, or reject it? Why?:
- Files changed (if any):
- Before/after behavior and test result:
- Screenshot or chat export (if the course allows it):

## Review checklist

- [ ] I personally ran Copilot on this project.
- [ ] I reviewed its suggestions and documented specific results.
- [ ] I re-ran the endpoint tests after making relevant changes.
- [ ] Any submitted Copilot claims reflect what I actually did.
