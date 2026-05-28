
using PeriodTracker;

namespace PeriodTrackerTests;

public interface ISeedDataProvider
{
    SeedData GetSeedData();
}

public class SeedData: ISeedDataProvider
{
    public List<AppState> AppStates {get; init;} = new();
    public List<Cycle> Cycles {get; init;} = new();

    public SeedData GetSeedData() => this;
}