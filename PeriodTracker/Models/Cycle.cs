using System.ComponentModel.DataAnnotations;

namespace PeriodTracker;

public class Cycle: IEquatable<Cycle>
{
    public DateTime RecordedDate {get; set;}

    [Key]
    public DateTime StartDate {get; set;}

    public bool Equals(Cycle? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return StartDate == other.StartDate;
    }

    public override int GetHashCode() => StartDate.GetHashCode();
}
