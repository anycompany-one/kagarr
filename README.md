# Kagarr

The missing *arr for games. Self-hosted game library manager and download automation.

> **Alpha** — Kagarr is functional but still early. Expect rough edges, missing features, and breaking changes between releases.

---

## What Is Kagarr?

Kagarr does for games what Sonarr does for TV and Radarr does for movies: search indexers, grab releases, send them to your download client, and auto-import the finished files into your library. It integrates with IGDB for metadata, Prowlarr-compatible Newznab indexers for search, and qBittorrent or SABnzbd for downloading.

## Features

- **IGDB metadata** — search and add games with cover art, summaries, ratings, and platform info
- **Newznab indexer support** — works with Prowlarr and any Newznab-compatible indexer
- **Download clients** — qBittorrent and SABnzbd integration with queue monitoring
- **Auto-import** — completed downloads are automatically detected, imported, and tracked
- **Wishlist** — add games and optionally set price thresholds for deal tracking
- **Deal tracking** — periodic checks against IsThereAnyDeal for price drops on wishlisted games
- **Activity queue** — real-time view of active downloads with progress bars
- **History** — full event log of grabs, imports, failures, and deletions
- **Discord notifications** — webhook alerts for grabs and deal matches
- **API key authentication** — optional, auto-generated if not set
- **Health checks** — system status page showing IGDB, indexer, and download client connectivity
- **Dark UI** — React frontend with a clean, responsive design

## Quick Start

### Docker (recommended)

```yaml
services:
  kagarr:
    image: ghcr.io/anycompany-one/kagarr:latest
    container_name: kagarr
    ports:
      - "6767:6767"
    environment:
      - KAGARR_IGDB_CLIENT_ID=your_twitch_client_id
      - KAGARR_IGDB_CLIENT_SECRET=your_twitch_client_secret
      # Optional:
      # - KAGARR_API_KEY=your_api_key
      # - KAGARR_DISCORD_WEBHOOK=https://discord.com/api/webhooks/...
      # - KAGARR_ITAD_API_KEY=your_isthereanydeal_key
      # - KAGARR_DEAL_CHECK_HOURS=6
      # - KAGARR_IMPORT_CHECK_SECONDS=60
    volumes:
      - ./config:/config
      - /path/to/games:/games
      - /path/to/downloads:/downloads
    restart: unless-stopped
```

### IGDB credentials

Kagarr requires a Twitch/IGDB API client to fetch game metadata:

1. Go to [dev.twitch.tv](https://dev.twitch.tv/console/apps) and create an application
2. Set the OAuth redirect to `http://localhost`
3. Copy the **Client ID** and generate a **Client Secret**
4. Pass them as `KAGARR_IGDB_CLIENT_ID` and `KAGARR_IGDB_CLIENT_SECRET`

## Configuration

After starting Kagarr, open `http://localhost:6767` and go to **Settings**.

### Indexers

Add a Newznab indexer (or connect to Prowlarr):

- **Name** — display label
- **URL** — base URL of the indexer (e.g. `http://prowlarr:9696/1/api`)
- **API Key** — your indexer API key
- **Categories** — Newznab category IDs for games (commonly `4050`)

### Download clients

Add qBittorrent or SABnzbd:

- **qBittorrent** — host, port, username, password
- **SABnzbd** — host, port, API key, category

Once configured, search for a game, click **Grab**, and Kagarr handles the rest.

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `KAGARR_IGDB_CLIENT_ID` | — | **Required.** Twitch client ID for IGDB metadata |
| `KAGARR_IGDB_CLIENT_SECRET` | — | **Required.** Twitch client secret |
| `KAGARR_API_KEY` | auto-generated | API key for authentication (passed via `X-Api-Key` header) |
| `KAGARR_DISCORD_WEBHOOK` | — | Discord webhook URL for notifications |
| `KAGARR_ITAD_API_KEY` | — | IsThereAnyDeal API key for deal tracking |
| `KAGARR_DEAL_CHECK_HOURS` | `6` | Hours between deal check cycles |
| `KAGARR_IMPORT_CHECK_SECONDS` | `60` | Seconds between auto-import polls |
| `KAGARR_DATA` | `/config` | Data directory for SQLite database and logs |

## Development

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- npm

### Build

```bash
# Backend
dotnet build src/Kagarr.sln

# Frontend
cd frontend && npm install && npm run build

# Run tests
dotnet test src/Kagarr.sln

# Run locally
dotnet run --project src/Kagarr.Console/Kagarr.Console.csproj -- --data=./dev-config
```

### Project Structure

```
src/
  Kagarr.Common/        # Shared base classes (ModelBase, RestResource)
  Kagarr.Core/           # Domain logic (games, indexers, download clients, jobs)
  Kagarr.Http/           # HTTP client infrastructure
  Kagarr.SignalR/        # Real-time hub for queue updates
  Kagarr.Api.V1/         # REST API controllers and resources
  Kagarr.Host/           # ASP.NET host, middleware, DI bootstrap
  Kagarr.Console/        # Entry point
  Kagarr.Core.Test/      # Unit tests (NUnit + Moq + FluentAssertions)
frontend/                # React 18 + TypeScript + Vite
```

## Backup

Kagarr automatically backs up `kagarr.db` to `kagarr.db.bak` before running database migrations on startup. For additional safety, back up the entire `/config` volume before upgrading to a new version.

## Roadmap

- Quality profiles (preferred release groups, size limits)
- RSS monitoring for automatic grabs
- Multi-platform game file organization
- Bulk import from existing libraries
- Notification agents beyond Discord (Telegram, email, Gotify)
- Prowlarr native sync (two-way indexer management)

## License

Kagarr is licensed under the [GNU General Public License v3.0](LICENSE), the same license used by Sonarr, Radarr, and Prowlarr.
