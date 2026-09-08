# Changelog

All notable changes to Kagarr will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.1.2-alpha] - 2026-09-08

### Fixed
- Database backups now use the SQLite online backup API; previous backups could miss WAL contents or be torn (#103)
- Media covers are served via `/api/v1/mediacover` and stored under the data path; they previously pointed at a dead route and were lost on container recreation (#107)
- Failed auto-imports back off between retries and give up after 5 attempts instead of retrying every minute forever (#105)
- A duplicate grab of the same release no longer permanently blocks that download's import (#108)
- IGDB token refresh validates the auth response and no longer loops on short-lived tokens (#110)

### Security
- Swagger docs (`/api/docs`) now require the API key (#104)
- Indexer and download-client APIs redact stored passwords/API keys; the UI round-trips redacted values without wiping credentials (#104)
- API key is header-only; `?apikey=` query authentication removed (#104)
- Anonymous `/api/v1/system/status` trimmed to app name and version (#104)
- Game titles are sanitized against path traversal and reserved characters on all platforms (#109)

### Changed
- All outbound HTTP uses pooled `IHttpClientFactory` clients and async I/O end-to-end; indexer searches fan out in parallel (#106)
- Frontend: React 19, TypeScript 7; settings pages consolidated on the shared API helper

## [0.1.1-alpha] - 2026-08-29

### Fixed
- Multi-file game imports no longer overwrite each other; existing target files are never silently deleted (#99)
- qBittorrent downloads are now tracked by info-hash, fixing auto-import never triggering for torrents (#100)
- Imported game files are persisted to the database and linked to their game (#101)
- SABnzbd transient states (verifying, repairing, extracting) are no longer treated as completed, preventing import of partially extracted files (#102)

### Changed
- Default port remapped from 6767 to 8585 to avoid collision with Bazarr
- Release tags now publish a versioned Docker image (`ghcr.io/anycompany-one/kagarr:<tag>`) in addition to `latest`
- Dependabot updates are grouped weekly to reduce PR noise
- Dependency updates: Microsoft.Extensions.* 10.0.11, Microsoft.Data.Sqlite 10.0.11, NLog 6.1.2, Swashbuckle 10.1.7, vite 8.2.2, i18next 26.4.0, react-i18next 17.0.12

### Security
- Microsoft.OpenApi bumped to 3.5.4, fixing GHSA-v5pm-xwqc-g5wc (circular schema references could terminate OpenAPI parsing)

### Documentation
- Unraid file-ownership guidance (`--user 99:100`) for the root-running container image

## [0.1.0-alpha] - 2025-02-15

### Added
- IGDB metadata search with cover art, summaries, ratings, and platform info
- Newznab/Torznab indexer support (Prowlarr-compatible)
- Download client integration (qBittorrent, SABnzbd) with queue monitoring
- Auto-import pipeline with completed download detection and library organization
- Hardlink support and configurable import modes (copy, move, hardlink)
- Scene release title parsing for automated matching
- Wishlist system with optional price threshold tracking
- Deal tracking via IsThereAnyDeal and Steam APIs
- Real-time activity queue via SignalR
- Full event history (grabs, imports, failures, deletions)
- Discord webhook notifications for grabs and deal matches
- Remote path mapping for cross-container setups
- API key authentication with constant-time comparison
- Health check dashboard (IGDB, indexer, download client connectivity)
- Configurable port and bind address (KAGARR_PORT, --port, --bind)
- IGDB, Steam, and ITAD API rate limiting
- Scheduled daily database backups with retention
- Internationalization support (en, pt-BR, de, fr, ja, ko, zh-CN, ru, pl)
- Dark-themed React frontend
- Docker image with multi-platform CI builds (ghcr.io)
- Unraid community template

### Security
- API key middleware with constant-time comparison
- Exception middleware prevents stack trace leakage
- Responsible disclosure policy (SECURITY.md)

[0.1.2-alpha]: https://github.com/anycompany-one/kagarr/releases/tag/v0.1.2-alpha
[0.1.1-alpha]: https://github.com/anycompany-one/kagarr/releases/tag/v0.1.1-alpha
[0.1.0-alpha]: https://github.com/anycompany-one/kagarr/releases/tag/v0.1.0-alpha
