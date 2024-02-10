

namespace PeriodTracker;

public enum AppStateProperty
{
    // NOTE: When adding or changing properties, a migration is needed.

    Unknown = 0,

    [DataType(typeof(int))]
    NotifyUpdateAvailableInterval = 1,

    [DataType(typeof(DateTime))]
    NotifyUpdateAvailableNextDate = 2,
}