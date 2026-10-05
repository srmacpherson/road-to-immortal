Developer run instructions
=========================

Purpose
-------
Short instructions to run the backend and frontend locally and to avoid solution-level TypeScript build issues in Visual Studio.

Backend (recommended)
---------------------
1. Build the backend project only (avoids solution-level TypeScript scans):

   dotnet build "backend\RoadToImmortal.Api\RoadToImmortal.Api.csproj"

2. Run the backend API locally:

   dotnet run --project "backend\RoadToImmortal.Api\RoadToImmortal.Api.csproj"

3. Run tests:

   dotnet test "backend\RoadToImmortal.Api.Tests\RoadToImmortal.Api.Tests.csproj"

Notes:
- The backend connects to PostgreSQL (DefaultConnection in backend/appsettings.json). For local development you can use Docker or set a connection string to an accessible Postgres instance.

Frontend (Vite + React)
-----------------------
1. Change into the frontend folder and install dependencies:

   cd frontend
   npm install

2. Start the Vite dev server:

   npm run dev

3. By default the frontend API wrapper points at http://127.0.0.1:5184 (see frontend/src/api/dashboardApi.ts). If you run the backend on a different URL or port, update API_BASE_URL in that file or replace it with an environment variable (Vite uses VITE_ prefixed env vars: import.meta.env.VITE_API_BASE_URL).

Why the solution build may fail
------------------------------
When building the entire Visual Studio solution or running dotnet build at the solution root, MSBuild in some environments attempts to compile TypeScript/TSX files and may not pick up the Vite/tsconfig settings used by the frontend. This can cause TSX parse errors (eg. "Cannot find name 'div'").

Workarounds
-----------
- Build the backend csproj directly (see commands above). This avoids scanning the frontend TypeScript files.
- Use the Vite dev server to build/run the frontend locally instead of relying on Visual Studio to compile the frontend.
- If you want Visual Studio to build the frontend automatically, consider adding an MSBuild target that runs npm build and consumes the output as static web assets. That is more invasive and environment-specific.

Files added to support local development
---------------------------------------
- Directory.Build.props — attempts to block MSBuild TypeScript compilation in the current environment.
- tsconfig.json (repo-level) — ensures JSX setting is available to tools that read workspace tsconfig.
- DEVELOPMENT.md (this file) — run instructions.

If you want, I can:
- Add Vite environment variables and replace the hardcoded API_BASE_URL with import.meta.env.VITE_API_BASE_URL, and add a small README change to show env usage.
- Add a small MSBuild target to run npm build and include the frontend as static web assets (more invasive).
