using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

[Keyless]
[EntityTypeConfiguration(typeof(CycleHistoryConfiguration))]
public class CycleHistory
{
    public DateTime StartDate { get; init; }
    public DateTime RecordedDate { get; init; }
    public int CycleLengthDays { get; init; }
}
