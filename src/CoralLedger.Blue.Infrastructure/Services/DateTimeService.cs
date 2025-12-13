using CoralLedger.Blue.Application.Common.Interfaces;

namespace CoralLedger.Blue.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
