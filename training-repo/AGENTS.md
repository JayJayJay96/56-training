# OrderHub — Agent Guide

## Scope

- Solution: `training-repo/OrderHub.sln`.
- Application code is in `training-repo/src`; tests are in `training-repo/tests`; training notes are in `documents`.
- Before analysing a project issue, locate the relevant `.sln` and read `PROJECT_LOGIC.md` beside it when that file exists.

## Architecture

- `OrderHub.Web`: MVC Controllers, ViewModels, Razor Views. Keep Controllers thin.
- `OrderHub.Core`: Domain models, interfaces, and business rules in services.
- `OrderHub.Infrastructure`: EF Core DbContext, repositories, migrations, and seed data.
- Only repositories access `OrderHubDbContext`. Views receive ViewModels, not domain entities.

## Engineering Conventions

- Use `decimal` for money.
- Apply price snapshots and customer-tier discounts exactly once.
- Use `ServiceResult<T>` for expected business failures; do not use exceptions for normal validation feedback.
- Preserve the existing pattern: Controller → Service → Repository.
- For user input, use DataAnnotations and ModelState so invalid input does not become a 500 response.

## Change Safety

- Do not change source, configuration, docs, data, or deployment files unless the user explicitly asks for a change.
- Do not edit `src/OrderHub.Infrastructure/Migrations/**`, connection strings, or install NuGet packages without explicit approval.
- Do not reset or drop the development database without explicit approval.
- Do not push Git changes without explicit approval.

## Verification

- Run tests from `training-repo`: `dotnet test OrderHub.sln --no-restore -m:1`.
- Run the web application: `dotnet run --project src/OrderHub.Web`.
- For a bug fix: reproduce or record the symptom, trace the data flow, make the smallest scoped fix, add a regression test, review the diff, run tests, then ask the user to verify the UI flow.

## Reporting

- For investigation or review, report: conclusion, impact, evidence with paths and lines, root-cause chain, suggested fix, and verification steps.
- State clearly when an item still needs UI, deployment, or production-data verification.