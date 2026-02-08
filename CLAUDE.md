# CLAUDE.md — Kagarr Project Instructions

## Project Identity

**Kagarr** is a self-hosted game library manager and download automation tool — the missing *arr for games. It is an open-source project under the **AnyCompany** portfolio (anycompany-one GitHub org).

- **License**: GPLv3 (consistent with Sonarr/Radarr)
- **Status**: Greenfield — building from scratch following *arr patterns
- **Owner**: Ronald Rodriguez (contact@anycompany.one)
- **Repo**: github.com/anycompany-one/kagarr

## Architecture Reference

Kagarr follows the same architectural patterns as Sonarr and Radarr. The reference codebases are in `./references/`:

```
references/
├── Sonarr/     # PRIMARY reference — most mature codebase
├── Radarr/     # SECONDARY — shows how Sonarr patterns adapt to different media
└── Prowlarr/   # TERTIARY — indexer/search integration patterns
```

**When building any Kagarr feature, ALWAYS check the equivalent implementation in Sonarr first, then Radarr.** Adapt the patterns, don't reinvent.

## Tech Stack (Must Follow)

| Layer | Technology | Notes |
|-------|-----------|-------|
| Backend | C# / .NET 8 | Match Sonarr's current target framework |
| Frontend | React + TypeScript | Match Sonarr's frontend/src pattern |
| Database | SQLite (default) | FluentMigrator for migrations, same as Sonarr |
| ORM | Dapper | Same as Sonarr — no Entity Framework |
| API | REST (v1) + SignalR | Real-time updates for download progress |
| DI Container | Same pattern as Sonarr | Convention-based registration |
| Logging | NLog | Same as Sonarr |
| Testing | NUnit + Moq + FluentAssertions | Same as Sonarr |

## Project Structure

Follow the Sonarr/NzbDrone naming convention but with Kagarr branding:

```
src/
├── Kagarr.Common/           # Shared utilities, disk operations, HTTP, env
├── Kagarr.Core/             # Business logic — THE HEART
│   ├── Games/               # Game entity, service, repository
│   ├── Platforms/           # Platform definitions (PC, PS, Xbox, Nintendo, Retro)
│   ├── MetadataSource/      # IGDB, RAWG, SteamDB, HowLongToBeat agents
│   ├── Download/            # Download client integration
│   │   └── Clients/        # SABnzbd, qBittorrent, Deluge, NZBGet
│   ├── Indexers/            # Prowlarr/Newznab/Torznab integration
│   ├── Organizer/           # File naming, folder structure, import
│   ├── MediaFiles/          # Game file detection, extraction, ISO handling
│   ├── Notifications/       # Webhooks, Discord, email
│   ├── Configuration/       # App settings
│   ├── Datastore/           # DB migrations, repositories
│   │   └── Migration/      # FluentMigrator migrations
│   └── Localization/        # i18n strings (en.json primary)
├── Kagarr.Api.V1/           # REST API controllers
├── Kagarr.Http/             # HTTP server, auth, middleware
├── Kagarr.Host/             # Application host, startup
├── Kagarr.Console/          # Console entry point
├── Kagarr.SignalR/          # Real-time hub
├── Kagarr.Update/           # Self-updater
├── Kagarr.Test.Common/      # Test utilities
└── Kagarr.Core.Test/        # Unit tests
frontend/
├── src/
│   ├── Components/          # React components
│   ├── Game/                # Game views (library, details, search)
│   ├── Settings/            # Settings pages
│   ├── System/              # System status, logs, updates
│   ├── Store/               # State management
│   └── typings/             # TypeScript interfaces
├── package.json
└── tsconfig.json
```

## Domain Model — Key Entities

### Game (equivalent to Sonarr's "Series" / Radarr's "Movie")

```
Game
├── Title (string)
├── CleanTitle (string, for matching)
├── SortTitle (string)
├── Year (int)
├── Overview (string)
├── IgdbId (int, primary metadata key)
├── SteamAppId (int?, optional)
├── Platform (enum: PC, PlayStation, Xbox, Nintendo, Retro, Multi)
├── Genres (List<string>)
├── Developer (string)
├── Publisher (string)
├── ReleaseDate (DateTime?)
├── CoverImage / Screenshots (MediaCover)
├── Path (string, root folder + game folder)
├── GameFileId (int?, linked GameFile)
├── Monitored (bool)
├── QualityProfileId (int)
├── Tags (List<int>)
├── AddOptions (AddGameOptions)
└── HowLongToBeat (HltbInfo? — main, extra, completionist hours)
```

### GameFile (equivalent to Sonarr's "EpisodeFile")

```
GameFile
├── GameId (int)
├── RelativePath (string)
├── Size (long)
├── DateAdded (DateTime)
├── Quality (QualityModel)
├── MediaInfo (GameMediaInfo — file type, compressed, installer, ISO)
├── Platform (enum)
└── ReleaseGroup (string)
```

### Platform Enum

```csharp
public enum GamePlatform
{
    Unknown = 0,
    PC = 1,
    PlayStation5 = 10,
    PlayStation4 = 11,
    Xbox_Series = 20,
    Xbox_One = 21,
    NintendoSwitch = 30,
    // Retro
    NES = 100,
    SNES = 101,
    N64 = 102,
    GameBoy = 103,
    GBA = 104,
    DS = 105,
    Genesis = 110,
    Dreamcast = 111,
    PS1 = 120,
    PS2 = 121,
    PSP = 122,
    // etc — extensible
}
```

## Metadata Sources (Priority Order)

1. **IGDB** (igdb.com) — Primary. Twitch-owned, comprehensive, free API with auth
2. **RAWG** (rawg.io) — Fallback. Good coverage, free tier
3. **SteamDB/Steam Store API** — PC-specific enrichment (pricing, reviews, tags)
4. **HowLongToBeat** — Playtime estimates (scraping, no official API)

## Key Differences from Sonarr/Radarr

| Concept | Sonarr | Radarr | Kagarr |
|---------|--------|--------|--------|
| Media unit | Series → Episodes | Movie | Game |
| Seasons | Yes | No | No (but DLC/Expansions as children?) |
| Quality | Video resolution | Video resolution | File type: Installer, ISO, Compressed, ROM, Portable |
| File types | .mkv, .mp4 | .mkv, .mp4 | .exe, .iso, .zip, .7z, .rar, .nsp, .xci, .rom, .bin, .cue |
| Post-processing | Rename, subtitle fetch | Rename | Extract, rename, verify, mount ISO |
| Metadata key | TVDB ID | TMDB ID | IGDB ID |
| Search pattern | Show + Season + Episode | Movie + Year | Game + Platform + Year |

## Naming Convention for Game Files

Default pattern: `{Game Title} ({Year}) [{Platform}]/{Game Title} ({Year}) [{Platform}].{ext}`

Example:
```
Baldur's Gate 3 (2023) [PC]/Baldur's Gate 3 (2023) [PC].iso
The Legend of Zelda - A Link to the Past (1991) [SNES]/zelda_alttp.sfc
```

## Quality Profiles (Game-Specific)

Instead of video quality (720p, 1080p, etc.), Kagarr uses file quality:

```
Preferred:
1. GOG Installer (DRM-free, clean installer)
2. Steam Rip (extracted Steam files)
3. ISO / Disc Image
4. Compressed Archive (.zip, .7z)
5. Scene Release (may need crack — lower trust)

For Retro/ROM:
1. No-Intro verified ROM
2. Redump verified disc image
3. GoodSet ROM
4. Unverified
```

## Docker (Day 1 Priority)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Follow linuxserver.io patterns for PUID/PGID
ENV PUID=1000 PGID=1000
EXPOSE 7878
VOLUME /config /games /downloads
```

Default port: **6767** (if available — check for conflicts)

## Build Commands

```bash
# Backend
dotnet build src/Kagarr.sln
dotnet test src/Kagarr.sln

# Frontend
cd frontend && yarn install && yarn build

# Docker
docker build -t kagarr .
docker run -d --name kagarr -p 6767:6767 -v /config:/config -v /games:/games kagarr
```

## API Design

Follow Sonarr's API patterns exactly:

```
GET    /api/v1/game              # List all games
GET    /api/v1/game/{id}         # Get game details
POST   /api/v1/game              # Add game to library
PUT    /api/v1/game/{id}         # Update game
DELETE /api/v1/game/{id}         # Remove game
GET    /api/v1/game/lookup       # Search metadata sources
POST   /api/v1/command           # Trigger actions (scan, search, rename)
GET    /api/v1/queue             # Current download queue
GET    /api/v1/history           # Download history
GET    /api/v1/gamefile          # List game files on disk
GET    /api/v1/platform          # List supported platforms
GET    /api/v1/tag               # Tags
GET    /api/v1/system/status     # Health check
```

## What NOT to Build (Out of Scope for MVP)

- Launcher integration (don't try to launch games — just manage files)
- Steam/Epic/GOG account sync (too complex, auth issues)
- Multiplayer / social features
- Achievements tracking
- Price tracking / deal alerts (different product)
- Cloud saves management

## Code Style

- Follow Sonarr's existing code style exactly
- Use the same `.editorconfig` as Sonarr
- Async/await throughout
- Dependency injection via constructor
- One class per file
- Interface for every service (ISomethingService)
- Repository pattern for data access

## Commit Convention

```
feat(games): add IGDB metadata lookup
fix(download): handle SABnzbd category for games
refactor(core): extract platform enum to shared
docs: update README with Docker instructions
chore: add GitHub Actions CI workflow
```

## Development Session Workflow

When starting a session with Claude Code:

1. State what you want to accomplish this session (e.g., "scaffold the project structure" or "implement IGDB metadata agent")
2. Reference this CLAUDE.md for context
3. Check Sonarr reference for the equivalent pattern before implementing (read locally from `references/`)
4. Build incrementally — get something compiling and running, then iterate
5. Always pass the compile gate: `dotnet build && dotnet test && docker build`
6. Use parallel Explore agents to study multiple reference patterns simultaneously
7. See WORKFLOW.md for detailed session workflow and prompt templates

## MVP Milestones (Rough Order)

### M1: Skeleton
- [ ] Project scaffolding matching Sonarr structure
- [ ] SQLite database with Game entity + migration
- [ ] Basic REST API (CRUD for games)
- [ ] Docker image that builds and starts
- [ ] Health check endpoint returns OK

### M2: Metadata
- [ ] IGDB API integration (search, get game details)
- [ ] Cover art / screenshot download and caching
- [ ] Game lookup endpoint works from API
- [ ] Basic frontend: library view with cover art grid

### M3: Indexers + Download
- [ ] Prowlarr integration (Newznab/Torznab)
- [ ] Search indexers for game releases
- [ ] Download client integration (qBittorrent first)
- [ ] Download queue tracking

### M4: Import + Organize
- [ ] Post-download import (detect, rename, move)
- [ ] File extraction (.zip, .7z, .rar)
- [ ] Folder organization by platform/title
- [ ] Manual import UI

### M5: Polish + Ship
- [ ] Settings UI
- [ ] Notification support (Discord webhook)
- [ ] Unraid Community App template
- [ ] README, screenshots, first release
- [ ] Post to r/selfhosted, r/homelab, *arr Discord
