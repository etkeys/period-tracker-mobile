using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeriodTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleHistoryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create view vCycleHistory as
select
    c.StartDate,
    c.RecordedDate,
    cast(coalesce(julianday(c.StartDate) - julianday(c.PreviousStartDate), 0) as integer) as CycleLengthDays
from (
    select
        c1.StartDate,
        c1.RecordedDate,
        (select max(c2.StartDate) from Cycles c2 where c2.StartDate < c1.StartDate) as PreviousStartDate
    from Cycles c1
) c;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
