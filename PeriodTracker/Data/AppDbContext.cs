using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

public partial class AppDbContext: DbContext
{
    private readonly IDbInitializationInfo _initInfo;

    public DbSet<AppState> AppState {get; set;} = null!;
    public DbSet<Cycle> Cycles {get; set;} = null!;

    public AppDbContext(IDbInitializationInfo initInfo){
        _initInfo = initInfo;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_initInfo.Database}");
    }
}