# CoralLedger Blue API Reference

## Overview

The CoralLedger Blue API provides RESTful endpoints for marine protected area monitoring, vessel tracking, coral bleaching alerts, and citizen science observations in The Bahamas.

**Base URL:** `https://api.coralledger.blue` (production) or `https://localhost:5001` (development)

**Interactive Documentation:** Available at `/scalar/v1` in development mode

**OpenAPI Specification:** Available at `/openapi/v1.json`

## Authentication

Currently, the API is open for read operations. Write operations and admin endpoints may require authentication in future versions.

## Rate Limiting

| Policy | Limit | Window |
|--------|-------|--------|
| Default | 100 requests | 1 minute |
| API | 60 requests | 1 minute |
| Strict (admin) | 10 requests | 1 minute |
| Global | 500 requests | 1 minute |

Rate limit headers are included in responses:
- `Retry-After`: Seconds until the rate limit resets

## API Endpoints

### Marine Protected Areas

#### GET /api/mpas
Get all Marine Protected Areas with summary information.

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "name": "Exuma Cays Land and Sea Park",
    "islandGroup": "Exumas",
    "protectionLevel": "NoTake",
    "areaSquareKm": 456.0,
    "establishedDate": "1958-01-01"
  }
]
```

#### GET /api/mpas/{id}
Get detailed information about a specific MPA.

**Parameters:**
- `id` (path, required): MPA GUID

**Response:** `200 OK` or `404 Not Found`

#### GET /api/mpas/geojson
Get all MPAs as GeoJSON FeatureCollection for map display.

**Query Parameters:**
- `resolution` (optional): `full`, `medium`, or `low` (default: medium)

**Response:** `200 OK` - GeoJSON FeatureCollection

#### GET /api/mpas/stats
Get aggregate statistics about Marine Protected Areas.

#### POST /api/mpas/{id}/sync-wdpa
Sync MPA boundary geometry from Protected Planet WDPA API.

---

### Coral Bleaching

#### GET /api/bleaching
Get bleaching alert history with optional filtering.

**Query Parameters:**
- `startDate` (optional): ISO 8601 date
- `endDate` (optional): ISO 8601 date
- `mpaId` (optional): Filter by specific MPA
- `minAlertLevel` (optional): Minimum alert level (0-5)

#### GET /api/bleaching/mpas/{mpaId}
Get bleaching history for a specific MPA.

#### GET /api/bleaching/current
Get current bleaching conditions across all MPAs.

#### POST /api/bleaching/sync
Trigger manual sync of NOAA Coral Reef Watch data.

---

### Vessel Tracking

#### GET /api/vessels
Get vessel activity events with filtering.

**Query Parameters:**
- `startDate`, `endDate`: Date range
- `mpaId`: Filter by MPA
- `eventType`: `Fishing`, `Transshipment`, `Anchoring`
- `pageSize`, `pageNumber`: Pagination

#### GET /api/vessels/mpas/{mpaId}
Get vessel events within a specific MPA.

#### GET /api/vessels/stats
Get vessel activity statistics.

#### POST /api/vessels/sync
Trigger manual sync of Global Fishing Watch data.

---

### AIS (Automatic Identification System)

#### GET /api/ais/positions
Get current AIS positions for vessels in Bahamas waters.

**Query Parameters:**
- `mpaId`: Filter by MPA boundary
- `mmsi`: Filter by vessel MMSI

#### GET /api/ais/tracks/{mmsi}
Get historical track for a specific vessel.

---

### Citizen Observations

#### GET /api/observations
Get approved citizen observations.

**Query Parameters:**
- `type`: `CoralBleaching`, `MarineDebris`, `WildlifeSighting`, `IllegalActivity`, `Other`
- `status`: `Pending`, `Approved`, `Rejected`
- `mpaId`: Filter by MPA

#### POST /api/observations
Submit a new citizen observation.

**Request Body:**
```json
{
  "type": "CoralBleaching",
  "latitude": 25.05,
  "longitude": -77.35,
  "description": "Observed bleaching on patch reef",
  "severity": "Moderate",
  "imageUrl": "https://..."
}
```

#### GET /api/observations/{id}
Get details of a specific observation.

#### PUT /api/observations/{id}/moderate
Approve or reject an observation (admin only).

---

### Alerts

#### GET /api/alerts
Get all active alerts.

**Query Parameters:**
- `type`: `Bleaching`, `VesselIntrusion`, `IllegalFishing`
- `severity`: `Low`, `Medium`, `High`, `Critical`
- `acknowledged`: `true` or `false`

#### GET /api/alerts/{id}
Get alert details.

#### PUT /api/alerts/{id}/acknowledge
Acknowledge an alert.

#### DELETE /api/alerts/{id}
Dismiss an alert.

---

### AI Insights

#### POST /api/ai/analyze-mpa
Get AI-powered analysis of an MPA's current status.

**Request Body:**
```json
{
  "mpaId": "guid"
}
```

#### POST /api/ai/predict-bleaching
Get bleaching risk prediction for a location.

#### POST /api/ai/chat
Interactive marine science assistant.

---

### Data Export

#### GET /api/export/mpas
Export MPA data in various formats.

**Query Parameters:**
- `format`: `csv`, `geojson`, `shapefile`

#### GET /api/export/bleaching
Export bleaching data.

#### GET /api/export/vessels
Export vessel activity data.

#### GET /api/export/observations
Export citizen observations.

---

### Admin

#### GET /api/admin/dashboard
Get admin dashboard statistics.

#### GET /api/admin/jobs
Get background job status.

#### POST /api/admin/jobs/{jobKey}/trigger
Manually trigger a background job.

#### GET /api/admin/cache/stats
Get cache statistics.

#### POST /api/admin/cache/clear
Clear all caches.

---

### Background Jobs

#### GET /api/jobs
Get status of all scheduled jobs.

#### GET /api/jobs/{jobKey}
Get details of a specific job.

#### POST /api/jobs/{jobKey}/trigger
Manually trigger a job execution.

---

## Real-Time Updates

### SignalR Hub

**Endpoint:** `/hubs/alerts`

**Events:**
- `NewAlert`: Fired when a new alert is generated
- `AlertUpdated`: Fired when an alert is acknowledged or modified
- `BleachingUpdate`: Fired when new bleaching data is synced

**Example (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/alerts")
    .build();

connection.on("NewAlert", (alert) => {
    console.log("New alert:", alert);
});

await connection.start();
```

## Error Responses

All error responses follow this format:

```json
{
  "error": "Error Type",
  "message": "Human-readable message",
  "details": {}
}
```

**Common Status Codes:**
- `400 Bad Request`: Invalid request parameters
- `404 Not Found`: Resource not found
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

## Health Endpoints

| Endpoint | Description |
|----------|-------------|
| `/health` | Overall health status |
| `/health/ready` | Readiness probe (all dependencies) |
| `/health/live` | Liveness probe (basic) |
| `/alive` | Simple alive check |

## Metrics

OpenTelemetry metrics are exposed for monitoring:

- `coralledger.mpa.queries`: MPA query count
- `coralledger.bleaching.alerts`: Bleaching alert count
- `coralledger.vessel.events`: Vessel event count
- `coralledger.observations.submitted`: Observation count
- `coralledger.api.requests`: API request count
- `coralledger.api.latency`: API latency histogram
- `coralledger.cache.hits/misses`: Cache performance
