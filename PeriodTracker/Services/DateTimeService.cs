
namespace PeriodTracker.Services;

public interface IDateTimeService
{
    DateTime Today { get; }
}

public class DateTimeService: IDateTimeService
{
    public DateTime Today => DateTime.Today;
}