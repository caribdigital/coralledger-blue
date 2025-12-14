# Changelog

All notable changes to CoralLedger Blue will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- Database initialization now uses `MigrateAsync()` instead of `EnsureCreatedAsync()` for proper migration support
- MPA name labels now display correctly on dark map tiles with proper Leaflet tooltip styling

### Added
- Sample fishing data endpoints for development (`POST /api/admin/dev/seed-fishing-events`)
- MPA labels on map polygons with CoralLedger Blue design system styling

## [1.0.0] - 2025-12-05

### Added - Sprint 3: Hardening & Spatial Excellence
- **Sprint 3.1**: Foundation Hardening
  - Comprehensive unit tests for Domain entities (400+ tests, 100% pass rate)
  - SpatialValidationService implementing Dr. Thorne's 10 GIS validation gates
  - User Secrets management for API keys (no secrets in source control)
  - GitHub Actions CI/CD pipeline for automated testing

- **Sprint 3.2**: Authoritative Data & Geometry Optimization
  - Protected Planet WDPA integration for official MPA boundaries
  - Multi-resolution geometry tiers (full/detail/medium/low)
  - `?resolution=` parameter on `/api/mpas/geojson` endpoint
  - BahamasSpatialConstants with EEZ bounding box validation
  - ReefHealthCalculator for spatial health metrics
  - MpaProximityService for containment/proximity analysis

- **Sprint 3.3**: Spatial Query Performance & Testing
  - UTM Zone 18N (SRID 32618) for accurate distance calculations
  - 34 comprehensive Map E2E tests with visual fidelity validation
  - MpaProximityService unit tests (25+ tests)
  - Point-in-polygon query optimization

### Changed
- Replaced Mapsui with Leaflet.js for map rendering (improved WebAssembly compatibility)
- Map component now uses local Leaflet library instead of CDN for reliability
- Updated Observations page to use LeafletMapComponent

### Documentation
- Added DEVELOPER.md for developer onboarding
- Updated .gitignore with Playwright, logs, and coverage patterns
- Documented potential improvement: Redis caching (US-3.3.5)

## [0.9.0] - 2024-12-04

### Added
- Comprehensive E2E testing framework with Playwright
- Visual fidelity tests for map rendering
- Navigation tests for all routes

## [0.8.0] - 2024-12-03

### Added
- Phase 9: Monitoring, CI/CD, and Documentation
- Species database integration
- User personas system
- AI-powered species classification

## [0.7.0] - 2024-12-02

### Added
- Phase 8: Mobile optimization, performance improvements, security hardening
- PWA enhancements

## [0.6.0] - 2024-12-01

### Added
- Phase 7: Data export functionality
- Admin panel
- Integration tests

## [0.5.0] - 2024-11-30

### Added
- Phase 6: Alert rules engine
- Real-time dashboard
- AIS vessel tracking

## [0.4.0] - 2024-11-29

### Added
- Phase 5: AI Intelligence with Semantic Kernel integration
- Natural language queries for marine data

## [0.3.0] - 2024-11-28

### Added
- Phase 4: Citizen Science features
- PWA offline support with service worker
- Azure Blob Storage photo upload
- CitizenObservation entity with CQRS
- ObservationFormComponent with geolocation
- Vessel map with MPA violation highlighting

## [0.2.0] - 2024-11-27

### Added
- Phase 3: Data Ingestion
- Global Fishing Watch API v3 integration
- NOAA Coral Reef Watch ERDDAP integration
- Protected Planet WDPA API integration
- Automated data pipelines with Quartz.NET
- Vessel tracking domain entities
- Bleaching alert domain entities

## [0.1.0] - 2024-11-26

### Added
- Phase 1: Spatial Foundation
- Clean Architecture setup with 6-project modular monolith
- PostGIS spatial database with PostgreSQL 16
- MPA entity with spatial boundaries (NetTopologySuite)
- 8 Bahamas MPA seed data
- Blazor Server web interface
- .NET Aspire orchestration

- Phase 2: Interactive Map
- Mapsui map component with OpenStreetMap tiles
- MPA polygon rendering with protection level styling
- GeoJSON API endpoints
- Blazor WebAssembly integration (Auto mode)
- Map/List view toggle
- Click-to-select MPA functionality
- Zoom-to-MPA on selection
- Selection highlight with info popup

---

**Repository:** https://github.com/caribdigital/coralledgerblue
**License:** MIT
