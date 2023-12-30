namespace PeriodTracker;

public class Cycle
{
    public DateTime RecordedDate {get; init;}
    public string RecordedDateText => RecordedDate.ToString("d");
    public DateTime StartDate {get; init;}
    public string StartDateText => StartDate.ToString("d");

}
