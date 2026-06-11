# Study Planner System

Web-based learning management and study planning platform built with ASP.NET Core 8.

## Project Overview

Study Planner System helps students organize subjects, create study tasks, schedule sessions, set learning goals, monitor progress, track achievements, and manage their learning process through a secure REST API.

## Features

- JWT authentication and role-based authorization (Administrator, Student)
- Full CRUD for subjects, tasks, sessions, goals, progress logs, and achievements
- Search, sorting, and pagination
- Statistics dashboard and weekly/monthly progress reports
- Automatic achievement unlocking
- Admin panel for user management and system reports
- Swagger API documentation and health check endpoint
- Seed data for demo and testing

## Architecture

Layered Clean Architecture:

```
StudyPlanner.API          -> Controllers, Middleware, Configuration
StudyPlanner.Infrastructure -> Services, Repositories, EF Core, Identity, AutoMapper
StudyPlanner.Core         -> Entities, DTOs, Interfaces, Enums, Exceptions
StudyPlanner.Tests        -> NUnit unit tests with Moq
```

### Database Diagram

```
ApplicationUser 1---* Subject 1---* StudyTask
      |                |
      |                *---* StudySession
      |                |
      *---* Goal       *---* ProgressLog
      *---* Achievement
      *---* RefreshToken
```

## Technologies

- ASP.NET Core 8.0
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- JWT Bearer Authentication
- AutoMapper
- NUnit + Moq
- Swagger / OpenAPI

## Installation

### Prerequisites

- Visual Studio 2022+
- .NET 8 SDK
- SQL Server or LocalDB

### Steps

1. Clone the repository
2. Open `Study planner System.sln` in Visual Studio
3. Set `StudyPlanner.API` as startup project
4. Update `StudyPlanner.API/appsettings.json` connection string if needed
5. Run the API (database migrates and seeds automatically)

```bash
cd StudyPlanner.API
dotnet run
```

Swagger UI: `https://localhost:7180/swagger`

Health check: `https://localhost:7180/health`

## Database Setup

The application uses EF Core migrations. On startup:

1. `Database.Migrate()` applies migrations
2. `DbInitializer` seeds roles, users, and sample data

### Seeded Accounts

| Role | Email | Password |
|------|-------|----------|
| Administrator | admin@studyplanner.com | Admin@12345 |
| Student | student1@studyplanner.com | Student@12345 |
| Student | student2@studyplanner.com | Student@12345 |
| Student | student3@studyplanner.com | Student@12345 |

## Authentication

1. `POST /api/auth/register` - create student account
2. `POST /api/auth/login` - receive JWT access + refresh tokens
3. Add header: `Authorization: Bearer {token}`
4. `POST /api/auth/refresh-token` - renew tokens
5. `POST /api/auth/logout` - revoke refresh tokens

## API Endpoints

### Auth
- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/logout`
- POST `/api/auth/refresh-token`

### Subjects
- GET/POST `/api/subjects`
- GET/PUT/DELETE `/api/subjects/{id}`

### Tasks
- GET/POST `/api/tasks`
- GET/PUT/DELETE `/api/tasks/{id}`
- POST `/api/tasks/{id}/complete`
- GET `/api/tasks/subject/{subjectId}`
- GET `/api/tasks/upcoming`

### Study Sessions
- GET/POST `/api/studysessions`
- GET/PUT/DELETE `/api/studysessions/{id}`
- GET `/api/studysessions/weekly`

### Goals
- GET/POST `/api/goals`
- GET/PUT/DELETE `/api/goals/{id}`
- PUT `/api/goals/{id}/progress`
- POST `/api/goals/{id}/complete`

### Progress
- GET/POST `/api/progress`
- GET/PUT/DELETE `/api/progress/{id}`
- GET `/api/progress/statistics`
- GET `/api/progress/weekly`
- GET `/api/progress/monthly`

### Achievements
- GET/POST `/api/achievements`
- GET/PUT/DELETE `/api/achievements/{id}`
- GET `/api/achievements/user`
- POST `/api/achievements/{id}/unlock`

### Admin (Administrator only)
- GET `/api/admin/users`
- GET `/api/admin/users/{userId}`
- DELETE `/api/admin/users/{userId}`
- GET `/api/admin/reports`
- GET `/api/admin/statistics`

## Testing

```bash
dotnet test StudyPlanner.Tests/StudyPlanner.Tests.csproj
```

With coverage:

```bash
dotnet test StudyPlanner.Tests/StudyPlanner.Tests.csproj --collect:"XPlat Code Coverage"
```

Tests cover services, controllers, validation, error handling, and authorization scenarios using Moq.

## Future Improvements

- Frontend SPA (React / Angular)
- Email notifications for deadlines
- Calendar integration
- Azure App Service deployment with Key Vault
- Redis caching for statistics
- Real-time notifications with SignalR
