using System.Diagnostics.Metrics;

namespace CoralLedger.Blue.Infrastructure.Telemetry;

/// <summary>
/// Custom metrics for CoralLedger Blue marine monitoring
/// </summary>
public sealed class MarineMetrics : IDisposable
{
    public const string MeterName = "CoralLedger.Marine";

    private readonly Meter _meter;

    // Counters
    private readonly Counter<long> _mpaQueriesCounter;
    private readonly Counter<long> _bleachingAlertsCounter;
    private readonly Counter<long> _vesselEventsCounter;
    private readonly Counter<long> _observationsCounter;
    private readonly Counter<long> _apiRequestsCounter;
    private readonly Counter<long> _cacheHitsCounter;
    private readonly Counter<long> _cacheMissesCounter;

    // Histograms
    private readonly Histogram<double> _apiLatencyHistogram;
    private readonly Histogram<double> _externalApiLatencyHistogram;
    private readonly Histogram<double> _databaseQueryLatencyHistogram;

    // Gauges (using ObservableGauges)
    private int _activeMpaCount;
    private int _activeAlertCount;
    private int _pendingObservationCount;
    private double _averageBleachingLevel;

    public MarineMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        // Counters
        _mpaQueriesCounter = _meter.CreateCounter<long>(
            "coralledger.mpa.queries",
            unit: "{queries}",
            description: "Number of MPA queries executed");

        _bleachingAlertsCounter = _meter.CreateCounter<long>(
            "coralledger.bleaching.alerts",
            unit: "{alerts}",
            description: "Number of bleaching alerts generated");

        _vesselEventsCounter = _meter.CreateCounter<long>(
            "coralledger.vessel.events",
            unit: "{events}",
            description: "Number of vessel events recorded");

        _observationsCounter = _meter.CreateCounter<long>(
            "coralledger.observations.submitted",
            unit: "{observations}",
            description: "Number of citizen observations submitted");

        _apiRequestsCounter = _meter.CreateCounter<long>(
            "coralledger.api.requests",
            unit: "{requests}",
            description: "Number of API requests");

        _cacheHitsCounter = _meter.CreateCounter<long>(
            "coralledger.cache.hits",
            unit: "{hits}",
            description: "Number of cache hits");

        _cacheMissesCounter = _meter.CreateCounter<long>(
            "coralledger.cache.misses",
            unit: "{misses}",
            description: "Number of cache misses");

        // Histograms
        _apiLatencyHistogram = _meter.CreateHistogram<double>(
            "coralledger.api.latency",
            unit: "ms",
            description: "API request latency in milliseconds");

        _externalApiLatencyHistogram = _meter.CreateHistogram<double>(
            "coralledger.external_api.latency",
            unit: "ms",
            description: "External API call latency in milliseconds");

        _databaseQueryLatencyHistogram = _meter.CreateHistogram<double>(
            "coralledger.db.query_latency",
            unit: "ms",
            description: "Database query latency in milliseconds");

        // Observable Gauges
        _meter.CreateObservableGauge(
            "coralledger.mpa.active_count",
            () => _activeMpaCount,
            unit: "{mpas}",
            description: "Number of active Marine Protected Areas");

        _meter.CreateObservableGauge(
            "coralledger.alerts.active_count",
            () => _activeAlertCount,
            unit: "{alerts}",
            description: "Number of active alerts");

        _meter.CreateObservableGauge(
            "coralledger.observations.pending_count",
            () => _pendingObservationCount,
            unit: "{observations}",
            description: "Number of pending observations awaiting moderation");

        _meter.CreateObservableGauge(
            "coralledger.bleaching.average_level",
            () => _averageBleachingLevel,
            unit: "{level}",
            description: "Average bleaching alert level across all MPAs");
    }

    // Counter methods
    public void RecordMpaQuery(string queryType = "list") =>
        _mpaQueriesCounter.Add(1, new KeyValuePair<string, object?>("query_type", queryType));

    public void RecordBleachingAlert(string severity, string islandGroup) =>
        _bleachingAlertsCounter.Add(1,
            new KeyValuePair<string, object?>("severity", severity),
            new KeyValuePair<string, object?>("island_group", islandGroup));

    public void RecordVesselEvent(string eventType, bool isInMpa) =>
        _vesselEventsCounter.Add(1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("in_mpa", isInMpa));

    public void RecordObservation(string type, string status) =>
        _observationsCounter.Add(1,
            new KeyValuePair<string, object?>("type", type),
            new KeyValuePair<string, object?>("status", status));

    public void RecordApiRequest(string endpoint, string method, int statusCode) =>
        _apiRequestsCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("status_code", statusCode));

    public void RecordCacheHit(string cacheKey) =>
        _cacheHitsCounter.Add(1, new KeyValuePair<string, object?>("cache_key", GetCacheKeyPrefix(cacheKey)));

    public void RecordCacheMiss(string cacheKey) =>
        _cacheMissesCounter.Add(1, new KeyValuePair<string, object?>("cache_key", GetCacheKeyPrefix(cacheKey)));

    // Histogram methods
    public void RecordApiLatency(double milliseconds, string endpoint) =>
        _apiLatencyHistogram.Record(milliseconds, new KeyValuePair<string, object?>("endpoint", endpoint));

    public void RecordExternalApiLatency(double milliseconds, string service) =>
        _externalApiLatencyHistogram.Record(milliseconds, new KeyValuePair<string, object?>("service", service));

    public void RecordDatabaseQueryLatency(double milliseconds, string queryType) =>
        _databaseQueryLatencyHistogram.Record(milliseconds, new KeyValuePair<string, object?>("query_type", queryType));

    // Gauge update methods
    public void SetActiveMpaCount(int count) => _activeMpaCount = count;
    public void SetActiveAlertCount(int count) => _activeAlertCount = count;
    public void SetPendingObservationCount(int count) => _pendingObservationCount = count;
    public void SetAverageBleachingLevel(double level) => _averageBleachingLevel = level;

    private static string GetCacheKeyPrefix(string cacheKey)
    {
        var colonIndex = cacheKey.IndexOf(':');
        return colonIndex > 0 ? cacheKey[..colonIndex] : cacheKey;
    }

    public void Dispose() => _meter.Dispose();
}
