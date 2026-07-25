# AGENTS.md

# Repository Overview

This repository contains multiple projects that together implement the
Boroughbridge Angling Club website.

The repository includes:

- A Blazor WebAssembly application.
- An ASP.NET Core Web API.
- Shared .NET class libraries (DTOs, models, utilities, etc.).
- A legacy ASP.NET Website project.
- A legacy Angular application used during migration.

Most development work should be carried out in the Blazor, API and shared
.NET projects.

---

# Build Environment

Codex Cloud executes in a Linux environment.

The repository contains a legacy ASP.NET Website project which cannot be
built by Linux MSBuild.

Therefore:

**Never restore or build the solution file.**

Do NOT execute:

```bash
dotnet restore AnglingClubWebSiteSolution.sln
dotnet build AnglingClubWebSiteSolution.sln
```

Doing so will fail with MSB4249 because of the legacy Website project.

---

# Building Projects

Instead of building the solution, restore and build only the SDK-style
projects affected by the current task.

Examples:

## Blazor UI changes

```bash
dotnet restore AnglingClubWebsite/AnglingClubWebsite.csproj
dotnet build AnglingClubWebsite/AnglingClubWebsite.csproj --no-restore
```

## Web API changes

Restore and build the Web API project.

## Shared library changes

Build every application project that references the changed shared library.

Typically this means:

- Blazor project
- Web API project

Building an application project automatically builds referenced shared
projects.

If a shared project is not referenced by an application being built,
build that project directly.

---

# Validation

Validate changes by building only the projects affected by the task.

Do not perform repository-wide builds.

Do not build unrelated projects simply because they exist.

When reporting completion, state exactly which projects were built.

---

# Project Scope

Only modify files that are relevant to the requested task.

Avoid unrelated refactoring.

Do not rename files, folders or projects unless explicitly requested.

Do not reorganise namespaces without a clear reason.

Avoid formatting-only changes.

Avoid introducing new packages unless required.

Do not upgrade package versions unless explicitly requested.

---

# Legacy Projects

The repository contains legacy projects used for compatibility.

Do not modify these unless the task explicitly requires it.

This includes:

- Legacy ASP.NET Website
- Angular application

---

# Angular

If Angular dependencies must be restored, always use:

```bash
cd angular-app
npm ci --legacy-peer-deps
```

Do not run:

```bash
npm audit fix
npm audit fix --force
```

Do not upgrade Angular packages unless explicitly requested.

---

# Coding Style

Follow the existing coding style throughout the repository.

## General

- Keep changes as small as practical.
- Match surrounding code style.
- Preserve existing architecture.
- Prefer consistency over personal preference.

## C#

Use:

- Allman brace style.
- Meaningful variable names.
- Async/await for asynchronous operations.
- Nullable reference types where already in use.
- Expression-bodied members only where they improve readability.

Avoid:

- Unnecessary comments.
- Unnecessary abstractions.
- Large refactoring.
- Mixing unrelated changes.

---

# Blazor

When modifying Blazor components:

- Prefer existing component patterns.
- Keep rendering logic simple.
- Avoid unnecessary re-rendering.
- Reuse existing services.
- Preserve existing routing conventions.

---

# API

When modifying the Web API:

- Preserve existing route conventions.
- Maintain backwards compatibility where practical.
- Reuse existing DTOs before creating new ones.
- Keep controller actions focused.

---

# Shared Projects

Shared DTOs and models are consumed by multiple applications.

When changing shared types:

- Consider impact on both client and server.
- Build all affected application projects.

---

# Performance

Avoid introducing unnecessary allocations.

Avoid repeated database or API calls where existing code already caches or
reuses results.

Prefer incremental changes over wholesale rewrites.

---

# Git

Keep commits focused on the requested task.

Do not modify unrelated files.

Do not create additional commits unless requested.

---

# If Unsure

If multiple implementation approaches are possible:

- Prefer the simplest solution.
- Prefer consistency with the existing codebase.
- Minimise the size of the change.

# Agent Behaviour

Before making changes:

1. Identify which SDK-style projects are affected.
2. Build only those projects.
3. Do not build the solution.
4. Keep changes focused on the user's request.
5. Explain any assumptions made.