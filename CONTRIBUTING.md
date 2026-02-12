# How to Contribute

We're always looking for people to help make Kagarr better. There are several ways to contribute.

## Bug Reports

If you find a bug, please [open an issue](https://github.com/anycompany-one/kagarr/issues/new?template=bug_report.yml) with as much detail as possible. Include your Kagarr version, OS, whether you're running Docker, and any relevant logs.

## Feature Requests

Have an idea? [Open a feature request](https://github.com/anycompany-one/kagarr/issues/new?template=feature_request.yml). Please search existing issues first to avoid duplicates.

## Development

Kagarr is written in C# (backend) and TypeScript (frontend). The backend is built on .NET 8, and the frontend uses React 18 with Vite.

### Tools Required

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- npm (included with Node.js)
- [Git](https://git-scm.com/downloads)
- An editor of your choice (VS Code, Visual Studio 2022, Rider, etc.)

### Getting Started

1. Fork the repository
2. Clone your fork

```bash
git clone https://github.com/YOUR_USERNAME/kagarr.git
cd kagarr
```

### Building the Backend

```bash
dotnet build src/Kagarr.sln
```

### Building the Frontend

```bash
cd frontend
npm install
npm run build
```

### Running Locally

```bash
dotnet run --project src/Kagarr.Console/Kagarr.Console.csproj -- --data=./dev-config
```

Open http://localhost:6767 in your browser.

### Running Tests

```bash
dotnet test src/Kagarr.sln
```

## Contributing Code

- If you're adding a feature that's already been requested, comment on the issue first so work isn't duplicated
- If you want to add something not already on the issues list, please open an issue to discuss it first
- Make meaningful commits, or squash them
- Feel free to open a pull request before work is complete — this lets us see progress and provide feedback early
- Add tests for any backend changes (NUnit + Moq + FluentAssertions)

### Code Style

- StyleCop analyzers are enforced with `TreatWarningsAsErrors=true` — your code won't compile if it violates the rules
- 4 spaces for indentation (no tabs)
- Commit with \*nix line endings
- Using directives must be alphabetically ordered (SA1210)

### Pull Requests

- Make pull requests to `main`
- One feature or bug fix per pull request
- Use a meaningful branch name:
  - `add-rss-monitoring` (good)
  - `fix-import-crash` (good)
  - `patch` (bad)
  - `main` (bad)
- Include a clear description of what changed and why
- We'll try to respond to pull requests as soon as possible

## Project Structure

```
src/
  Kagarr.Common/        # Shared base classes (ModelBase, RestResource)
  Kagarr.Core/           # Domain logic (games, indexers, download clients, jobs)
  Kagarr.Http/           # HTTP client infrastructure
  Kagarr.SignalR/        # Real-time hub for queue updates
  Kagarr.Api.V1/         # REST API controllers and resources
  Kagarr.Host/           # ASP.NET host, middleware, DI bootstrap
  Kagarr.Console/        # Entry point
  Kagarr.Core.Test/      # Unit tests
frontend/                # React 18 + TypeScript + Vite
```
