

namespace PeriodTracker;

public enum AppStateProperty
{
    // NOTE: When adding or changing properties, a migration is needed.

    [DataType(typeof(int))]
    NotifyUpdateAvailableInterval,

    [DataType(typeof(DateTime))]
    NotifyUpdateAvailableNextDate,
}