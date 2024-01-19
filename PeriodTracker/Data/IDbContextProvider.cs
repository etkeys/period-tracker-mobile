namespace PeriodTracker;

public interface IDbContextProvider
{
    Task<Repository> GetContext();
}
