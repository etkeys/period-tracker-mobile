using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PeriodTracker;

public class CycleHistoryConfiguration: IEntityTypeConfiguration<CycleHistory>
{
    public void Configure(EntityTypeBuilder<CycleHistory> builder)
    {
        builder.ToView($"v{nameof(CycleHistory)}");
    }
}