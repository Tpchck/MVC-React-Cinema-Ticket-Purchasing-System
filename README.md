# CinemaTAPS

A cinema ticket purchasing web application built as a university lab project (EGUI, WUT).
Covers user management, session scheduling, seat reservation, and concurrency handling.

---

## Tech Stack

**Backend** — ASP.NET Core 10, Entity Framework Core, PostgreSQL, JWT
**Frontend** — React 18, TypeScript, Vite, Bootstrap 5, Zustand

---

## Features

- User registration and profile editing
- Admin panel: create and delete screenings
- Seat map with real-time occupancy display
- Seat booking and cancellation
- Concurrency conflict handling (optimistic locking on user edits, unique index on seat reservations)
- Two run modes: development (two servers) and production (single server)

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- PostgreSQL

Configure the connection string and JWT secret in `CinemaTAPS.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CinemaDb;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-32-characters"
  }
}
```

Apply migrations to create and seed the database:

```bash
cd CinemaTAPS.API
dotnet ef database update
```

---

### Development mode

```bash
# Terminal 1 — API
cd CinemaTAPS.API
dotnet run

# Terminal 2 — React dev server
cd CinemaTAPS.Client
npm install
npm run dev
```

Open `http://localhost:5173`

---

### Production mode (single server)

```bash
cd CinemaTAPS.Client
npm run build

cd ../CinemaTAPS.API
dotnet run
```

Open `http://localhost:5183`

---

## Default Admin Account

Can be hardcoded manualy.

---

## Project Structure

```
CinemaTAPS.ASP.MVC.REACT/
├── CinemaTAPS.API/        # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── DTOs/
│   └── Data/              # ApplicationDbContext, migrations
└── CinemaTAPS.Client/     # React frontend
    └── src/
        ├── pages/
        ├── components/
        ├── store/         # Zustand auth store
        └── api/           # HTTP client wrapper
```

---

*Warsaw University of Technology — EGUI Laboratory, Semester 6*
