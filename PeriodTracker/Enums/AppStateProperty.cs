

namespace PeriodTracker;

public enum AppStateProperty
{
    // NOTE: When adding or changing properties, a migration is needed.

    Unknown = 0,

    NotifyUpdateAvailableInterval = 1,

    NotifyUpdateAvailableNextDate = 2,
}