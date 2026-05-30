# School Portal Management System

A containerized .NET 8 microservices application for managing students and grades, built with ASP.NET Core MVC, Entity Framework Core, SQL Server, and Docker.

---

## Architecture

```
Browser (Teacher)          Browser (Admin)
       |                          |
       v                          v
+-----------------+     +----------------------+
|  Students MVC   |<----|     Grades MVC       |
|  Port: 5001     |     |  Port: 5002          |
|                 |     |  (calls Students via |
|  - List         |     |   HttpClient)        |
|  - Add          |     |  - List Grades       |
|  - Edit         |     |  - Add Grade         |
|  - Delete       |     |  - Edit / Delete     |
+-----------------+     +----------------------+
        |                          |
        +------------+-------------+
                     |
        +------------------------+
        |   SQL Server Container |
        |    (Shared Database)   |
        |    + Docker Volume     |
        +------------------------+
```

- **students-mvc** — Source of truth for all student data. Exposes a JSON endpoint `/api/students` consumed by the Grades service.
- **grades-mvc** — Manages grade records. Fetches student names from the Students service via HttpClient. Uses a GradeViewModel to combine data from both sources.
- **sqlserver** — Shared SQL Server instance running in Docker with a named volume for data persistence.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2022 |
| Containerization | Docker + Docker Compose |
| Frontend | Razor Views + Custom CSS |
| HTTP Communication | HttpClient (typed client) |

---

## Project Structure

```
SchoolPortalManagementSystem/
├── docker-compose.yml
├── students-mvc/
│   ├── Controllers/
│   │   └── StudentsController.cs     # CRUD + /api/students JSON endpoint
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Models/
│   │   └── Student.cs
│   ├── Migrations/
│   ├── Views/Students/               # Index, Create, Edit, Delete, Details
│   ├── Dockerfile
│   └── Program.cs
└── grades-mvc/
    ├── Controllers/
    │   └── GradesController.cs       # CRUD + HttpClient calls
    ├── Data/
    │   └── AppDbContext.cs
    ├── Models/
    │   └── Grade.cs                  # No FK to Student — just int StudentId
    ├── ViewModels/
    │   └── GradeViewModel.cs         # Combines Grade + StudentFullName
    ├── Services/
    │   ├── StudentsApiClient.cs      # Typed HttpClient wrapper
    │   └── StudentDto.cs
    ├── Migrations/
    ├── Views/Grades/                 # Index, Create, Edit, Delete, Details
    ├── Dockerfile
    └── Program.cs
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Step 1 — Clone the repository

```bash
git clone https://github.com/AhmedAlaa610/SchoolPortalManagementSystem.git
cd SchoolPortalManagementSystem
```

### Step 2 — Create EF Core Migrations

Run once before the first Docker build:

```bash
# Students service
cd students-mvc
dotnet ef migrations add InitialCreate
cd ..

# Grades service
cd grades-mvc
dotnet ef migrations add InitialCreate
cd ..
```

Migrations are applied automatically on startup via `db.Database.Migrate()` — no manual steps needed after `docker compose up`.

### Step 3 — Run with Docker Compose

```bash
docker-compose up --build
```

### Step 4 — Open in Browser

| Service | URL |
|---|---|
| Students App | http://localhost:5001 |
| Grades App | http://localhost:5002 |
| Students JSON API | http://localhost:5001/api/students |

---

## Features

### Students Service (Port 5001)

- Full CRUD for student records (Create, Read, Update, Delete)
- Student list with avatar initials
- JSON endpoint `/api/students` for inter-service communication

### Grades Service (Port 5002)

- Full CRUD for grade records
- Student dropdown populated live from the Students service
- Color-coded score badges based on score range
- Graceful degradation — if the Students service is offline, the Grades page still loads and shows Student IDs with a warning banner instead of crashing

### Infrastructure

- Docker Compose brings up all 3 services with one command
- Named volume (`sqldata`) persists database across `docker compose down` and `up`
- Custom Docker network (`school-net`) enables service discovery by name (e.g. `http://students-mvc:8080`)
- Health check on SQL Server — MVC apps wait for the database to be ready before starting
- Retry logic in `Program.cs` — handles SQL Server cold-start delays gracefully
- Multi-stage Dockerfiles — SDK image for build, smaller ASP.NET runtime image for production

---

## Design Decisions

### Why ViewModel?

`GradeViewModel` combines data from two sources — the local database (`Grade`) and the Students HTTP API (`StudentFullName`). The raw `Grade` entity has no `StudentFullName` field. Passing the entity directly to the view would require the view to make HTTP calls, violating separation of concerns. The ViewModel's single responsibility is to carry exactly what the view needs to render.

### Why no FK constraint between Grades and Students?

The two services own independent databases. A foreign key constraint across service boundaries would create tight coupling — if the Students DB is unavailable, the Grades DB would break too. Instead, `Grade.StudentId` is a plain `int`. The relationship is maintained at the application layer via HTTP, not at the DB layer.

### Why Docker service names instead of localhost?

Inside a Docker Compose network, containers communicate using their service name as the hostname (e.g. `http://students-mvc:8080`). Using `localhost` would point to the container itself, not another service. The `StudentsServiceUrl` is injected via environment variable so it works both locally (`http://localhost:5001`) and in Docker (`http://students-mvc:8080`).

---

## Testing the Setup

After running `docker-compose up --build`:

1. Open `localhost:5001` — Students app loads
2. Add a few students
3. Open `localhost:5002` — Grades app loads
4. Click Add Grade — student dropdown shows real student names
5. Add a grade — student name appears in the list (not just an ID)
6. Run `docker-compose down` then `docker-compose up` — data is still there (volume works)
7. Stop the `students-mvc` container — reload Grades index — warning banner appears, no crash

---

## Environment Variables

| Variable | Service | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Both | SQL Server connection string |
| `StudentsServiceUrl` | grades-mvc | Base URL of the Students service |
| `ASPNETCORE_URLS` | Both | ASP.NET Core listening URL inside container |

---

## Troubleshooting

**Containers exit with code 139 on first run**

SQL Server takes 15–30 seconds to initialize. The retry logic handles this automatically, but if it fails:

```bash
docker-compose restart students-mvc grades-mvc
```

**`dotnet ef` command not found**

```bash
dotnet tool install --global dotnet-ef
```

**Port already in use**

Change `5001:8080` or `5002:8080` in `docker-compose.yml` to any free port.

---

## License

This project is for educational purposes.
