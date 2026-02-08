# Kagarr Development Workflow Guide

## Overview

This is the accelerated development workflow for Kagarr, optimized for continuous Claude Code (Opus 4.6) agent-driven development. All milestones from CLAUDE.md are preserved — the pacing is aggressive and parallelized instead of spread across weekends.

---

## Pre-Session Setup (One-Time)

1. Ensure reference repos are cloned locally:
   ```bash
   cd ~/kagarr/references/Sonarr && git pull
   cd ~/kagarr/references/Radarr && git pull
   cd ~/kagarr/references/Prowlarr && git pull
   ```
2. Confirm `references/` is in `.gitignore` (never push reference repos)
3. Read CLAUDE.md for full project spec

---

## Session Start

```bash
cd ~/kagarr
claude
```

First message to Claude Code:
```
Read CLAUDE.md. Today's goal: [YOUR MILESTONE].
Before writing any code, check how Sonarr implements the equivalent
in references/Sonarr/. Show me the Sonarr pattern first, then
we'll adapt it for Kagarr.
```

---

## Core Development Loop

### Reference-First Pattern (Always)

1. **Claude Code reads** the Sonarr/Radarr reference for the pattern (locally, no web fetches)
2. **You review** the pattern — confirm or redirect
3. **Claude Code implements** the Kagarr version
4. **Compile gate** — must pass before moving on
5. **Commit** with conventional commit message

### Agent Parallelization Strategy

Use subagents for independent exploration and implementation:

- **Explore agents** (up to 3 in parallel): Deep-dive into reference repos for patterns
- **General-purpose agents**: Handle multi-step implementation tasks
- **Plan agents**: Design complex feature implementations before coding

Example: While one agent explores Sonarr's metadata pattern, another can scaffold the project structure, and a third can set up the build system.

### Compile Gate (Between Every Phase)

Every phase must pass before proceeding:
```bash
dotnet build src/Kagarr.sln
dotnet test src/Kagarr.sln
docker build -t kagarr .
```

---

## Key Claude Code Prompts

**Scaffolding a new feature:**
```
Look at how Sonarr implements [Download Clients / Metadata / Indexers]
in references/Sonarr/src/NzbDrone.Core/[folder]/.
Show me the key files and interfaces, then scaffold the Kagarr equivalent.
```

**When stuck on a pattern:**
```
Show me the Sonarr implementation of [specific thing] across these files:
- references/Sonarr/src/NzbDrone.Core/[path]
- references/Radarr/src/NzbDrone.Core/[path]
What's the common pattern? Adapt it for Kagarr.
```

**API endpoint:**
```
Look at references/Sonarr/src/Sonarr.Api.V3/[Controller].cs
and create the equivalent Kagarr.Api.V1 controller for [entity].
Follow the exact same patterns for pagination, filtering, and validation.
```

**Database migration:**
```
Look at references/Sonarr/src/NzbDrone.Core/Datastore/Migration/
Show me how they structure a migration, then create Migration 001
for the Kagarr Game table with the fields from CLAUDE.md.
```

---

## Session Prompt Templates

### M1: Project Scaffolding

```
Read CLAUDE.md. Today we scaffold the entire Kagarr project structure.

Step 1: Look at references/Sonarr/ top-level structure — solution file,
project layout, build scripts.

Step 2: Create the Kagarr.sln and all project stubs matching the
structure in CLAUDE.md.

Step 3: Set up the Game entity, initial database migration, and a
basic health check API endpoint.

Step 4: Create a Dockerfile that builds and runs.

Goal: `docker run kagarr` shows a web UI (even if it's just
"Kagarr is running" on port 6767).
```

### M2: IGDB Metadata

```
Read CLAUDE.md. Today we implement the IGDB metadata agent.

Step 1: Look at how Sonarr implements SkyHook/TVDB metadata in
references/Sonarr/src/NzbDrone.Core/MetadataSource/

Step 2: Create Kagarr.Core/MetadataSource/ with:
- IGameMetadataService interface
- IgdbMetadataService implementation
- DTOs for IGDB API responses
- Cover art download and caching

Step 3: Wire up the /api/v1/game/lookup endpoint.

Step 4: Test: search for "Baldur's Gate 3" and get back
metadata + cover art.
```

### M3: Indexer Integration

```
Read CLAUDE.md. Today we implement Prowlarr/indexer integration.

Step 1: Look at references/Prowlarr/src/ for how it exposes
Newznab/Torznab APIs, and look at Sonarr's indexer client code in
references/Sonarr/src/NzbDrone.Core/Indexers/

Step 2: Implement Kagarr's indexer integration:
- Newznab client (Usenet)
- Torznab client (Torrents)
- Prowlarr sync integration

Step 3: Search for a game through indexers and display results.
```

### M3b: Download Client

```
Read CLAUDE.md. Today we implement download client integration.

Step 1: Look at references/Sonarr/src/NzbDrone.Core/Download/Clients/
Focus on qBittorrent and SABnzbd implementations.

Step 2: Implement for Kagarr:
- qBittorrent client (priority)
- Download queue tracking
- Category management ("kagarr" category)

Step 3: Wire up: search -> select release -> send to download client ->
track in queue.
```

---

## Efficiency Maximizers

### Let Claude Code Handle:
- Boilerplate scaffolding (project files, DI registration, migrations)
- API controller CRUD (follow Sonarr patterns)
- Unit test scaffolding
- Dockerfile and docker-compose
- GitHub Actions CI workflow
- Parallel reference exploration via subagents
- Multi-file refactoring and pattern adaptation

### You Focus On:
- Domain decisions (what makes games different from movies/TV)
- UX decisions (how should the library view look?)
- Integration testing (does it actually work on your Unraid?)
- Community strategy (what to post, where, when)
- Reviewing and approving agent-proposed patterns

### Time Savers:
- **Don't** write CSS from scratch — use Sonarr's existing frontend as a starting point
- **Don't** implement auth from scratch — copy Sonarr's auth system
- **Don't** build a custom ORM — use Dapper like Sonarr does
- **Do** copy Sonarr's `.editorconfig`, `global.json`, and build scripts verbatim
- **Do** test on your actual Unraid setup after each milestone
- **Do** use parallel agents to explore multiple reference patterns simultaneously
- **Do** keep reference repos local — read them directly instead of web fetching

---

## Progress Tracking

Update this section after each milestone:

| Phase | Date | Milestone | Status | Notes |
|-------|------|-----------|--------|-------|
| 1 | | M1: Skeleton | ⬜ | |
| 2 | | M2: Metadata | ⬜ | |
| 3 | | M3: Indexers | ⬜ | |
| 4 | | M3b: Download | ⬜ | |
| 5 | | M4: Import | ⬜ | |
| 6 | | M4b: Organize | ⬜ | |
| 7 | | M5: Polish | ⬜ | |
| 8 | | M5b: Ship! | ⬜ | |
