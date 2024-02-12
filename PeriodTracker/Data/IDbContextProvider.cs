namespace PeriodTracker;

public interface IDbContextProvider
{
    Task<AppDbContext> GetContext();
}
