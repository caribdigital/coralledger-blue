# Changelog

All notable changes to CoralLedger Blue will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Replaced Mapsui with Leaflet.js for map rendering (improved WebAssembly compatibility)
- Map component now uses local Leaflet library instead of CDN for reliability
- Updated Observations page to use LeafletMapComponent

### Added
- LeafletMapComponent with full MPA visualization support
- JavaScript interop for Leaflet map controls
- Local Leaflet.js library (v1.9.4) in wwwroot/lib

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
