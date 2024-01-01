namespace PeriodTracker;

public class Cycle: IEquatable<Cycle>
{
    public DateTime RecordedDate {get; init;}
    public string RecordedDateText => RecordedDate.ToString("d");
    public DateTime StartDate {get; init;}
    public string StartDateText => StartDate.ToString("d");

    public bool Equals(Cycle? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return StartDate == other.StartDate;
    }

    public override int GetHashCode() => StartDate.GetHashCode();
}
