# Changelog

All notable changes to Kagarr will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

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

[0.1.0-alpha]: https://github.com/anycompany-one/kagarr/releases/tag/v0.1.0-alpha
