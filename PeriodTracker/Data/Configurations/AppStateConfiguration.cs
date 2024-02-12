
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PeriodTracker;

public class AppStateConfiguration : IEntityTypeConfiguration<AppState>
{
    public void Configure(EntityTypeBuilder<AppState> builder){
        builder.HasData(
            new {
                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableInterval,
                Value = "2"
            },
            new {
                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableNextDate,
                Value = DateTime.Parse("2024-01-01").ToString("s")
            }
        );
    }
}