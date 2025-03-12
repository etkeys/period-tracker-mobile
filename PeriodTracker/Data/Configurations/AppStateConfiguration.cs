
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PeriodTracker;

public class AppStateConfiguration : IEntityTypeConfiguration<AppState>
{
    public static readonly AppState[] SeedData = [
            new AppState {
                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableInterval,
                Value = "2"
            },
            new AppState {
                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableNextDate,
                Value = DateTime.Parse("2024-01-01").ToString("s")
            }
    ];

    public void Configure(EntityTypeBuilder<AppState> builder){
        builder.HasData(SeedData);
    }
}