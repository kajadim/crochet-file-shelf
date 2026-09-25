# Crochet File Shelf

A web application for keeping crochet projects in one place: patterns drawn as colored matrices, video tutorials,
saved links, your own yarn palette, and the option to share a project with other people and work on it together.

Built as a seminar project for the course Web Programming 2.

## Features

**Works and folders**
- Nested folders. Each folder holds three kinds of works:
  - **Matrix**: a crochet chart you paint cell by cell with your own yarn colors.
  - **Video**: a tutorial from YouTube, TikTok, Instagram or Pinterest, shown in an embedded player, with a start time.
  - **Site**: a saved link with notes.
- Search across work names, descriptions and comments, with filters for type, yarn color, video platform and sharing.

**Matrix editor**
- Paint, erase, zoom, mark where you stopped (position) and the row you are on, add or remove rows and columns.
- Legend with the number of cells per color, Excel export and import (`.xlsx`).
- Keyboard navigation (arrows, space, Delete, and shortcuts for position and row) and a status that shows when changes are saved.

**Yarn palette**
- Your personal colors with name, hex code and notes, searched and sorted by the server.
- Shows which works use each color, with links to them.

**Sharing and collaboration**
- Share a work with an invitation code and choose whether people can only view or also edit.
- Members work on the same matrix at the same time: changes, position and who is present appear live (SignalR). If two people change the same cell, the last change wins.
- Real-time notifications (someone joined, someone commented, access removed), each with a link to the work.
- Rich-text comments; dangerous HTML is removed on the server.

**Accounts**
- Registration with an emailed verification code, sign-in, password reset.
- Profile with first and last name, a unique username, a short bio and a profile picture.

**Interface**
- English, Serbian, German and French.
- Light and dark theme, responsive layout down to phone size, keyboard and screen reader support.

## Technology

| Part | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, Entity Framework Core (code first), SignalR |
| Database | PostgreSQL 16 (in Docker) |
| Authentication | JWT access token, refresh token in a cookie, emailed verification codes |
| Frontend | Angular 21 (standalone components, signals), PrimeNG, Transloco (translations), Quill (comment editor) |
| Styling | SCSS with CSS variables (no CSS framework) |
| Tests | Java 17, JUnit 5, Selenium, Maven (see [`tests/`](tests/README.md)) |
| Containers | Docker Compose: database, backend, frontend (nginx) and the tests with a Selenium browser |

## Project structure

```
backend/     ASP.NET Core API (Controllers, Services, Repository, Models, Migrations, Hubs)
frontend/    Angular application
tests/       Java tests (API, browser and combined flows)
docker-compose.yml   PostgreSQL, and optionally the whole application and the tests (see below)
```

## Running it locally

### Requirements

- Docker
- .NET 8 SDK
- Node.js 22 with npm
- A Gmail account with an app password, only if you want to register new users (the verification code is sent by email)

### 1. Database

```bash
docker compose up -d
```

This starts PostgreSQL on port `5433` (database `crochetfileshelf`, user `postgres`, password `password`).

### 2. Backend

Create `backend/appsettings.Development.json` from the example and fill in your values:

```bash
cd backend
cp appsettings.Development.json.example appsettings.Development.json
```

| Setting | Meaning |
|---|---|
| `ConnectionStrings:CrochetFileShelfDb` | Use `password` as the password to match `docker-compose.yml` |
| `Jwt:Secret` | Any long random string |
| `Email:*` | Gmail address and app password used to send verification codes |

Create the tables and start the API:

```bash
dotnet tool install --global dotnet-ef     # once
dotnet ef database update
dotnet run --launch-profile https          # https://localhost:7065
```

You can also run the `backend` project from Visual Studio. The Swagger page is available at `/swagger` in development.

### 3. Frontend

```bash
cd frontend
npm install
npm start                                  # http://localhost:4200
```

The development server forwards `/api` and `/hubs` to the backend on `https://localhost:7065`
(see `frontend/proxy.conf.json`).

Open `http://localhost:4200`, register an account, enter the code from the email and sign in.

## Running everything in Docker

Apart from the database, the backend, the frontend and the tests can run in containers too. The commands below are run in the project root.

```bash
docker compose up -d                                  # only the database (everyday development)
docker compose --profile app up -d --build            # the whole application: http://localhost:8080
docker compose --profile test run --rm tests          # the automated tests, with a browser in its own container
docker compose --profile app --profile test down      # stop and remove the containers
```

- The containerised application uses its own database (`crochetfileshelf_docker`) on the same PostgreSQL server, so the
  development data is never touched.
- The backend creates its tables at start-up (`Database__MigrateOnStartup`); the frontend is built and served by nginx,
  which also forwards `/api` and `/hubs` to the backend.
- To register users in the containerised application, put the Gmail settings in a `.env` file (see `.env.example`).

## Tests

```bash
cd tests
mvn test -Dtest.groups=api     # backend only, no browser
mvn test                       # everything, including browser tests
```

The easiest way is the Docker command above (`docker compose --profile test run --rm tests`), which needs nothing but Docker.
Running them on your own machine needs the application running and two registered accounts. All steps, settings and what is
covered are described in [`tests/README.md`](tests/README.md).

## Good to know

- Some Instagram videos (usually because of music rights) cannot be played outside Instagram. The application detects this
  and offers a button to open the original instead.
- The start time works only for YouTube. For the other platforms it is shown as a reminder.
- Matrices are limited to 200 × 200 cells and avatars to a 300 KB JPEG.
