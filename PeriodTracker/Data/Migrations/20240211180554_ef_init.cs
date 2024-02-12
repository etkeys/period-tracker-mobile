using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PeriodTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class ef_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SaveExistingData(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "AppState",
                columns: table => new
                {
                    AppStatePropertyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", unicode: false, maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppState", x => x.AppStatePropertyId);
                });

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cycles", x => x.StartDate);
                });

            migrationBuilder.InsertData(
                table: "AppState",
                columns: new[] { "AppStatePropertyId", "Value" },
                values: new object[,]
                {
                    { 1, "2" },
                    { 2, "2024-01-01T00:00:00" }
                });

            RestoreExistingData(migrationBuilder);
        }

        private void RestoreExistingData(MigrationBuilder migrationBuilder){
            var queries = new []{
                @"INSERT INTO Cycles (StartDate, RecordedDate)
                SELECT StartDate, RecordedDate
                FROM temp_Cycles",

                "DROP TABLE temp_Cycles",
            };

            foreach(var query in queries)
                migrationBuilder.Sql(query);
        }

        private void SaveExistingData(MigrationBuilder migrationBuilder){
            var queries = new []{
                // Have to create the original definition of Cycles
                // because if we don't, new installs will fail when
                // we apply this migration.
                @"CREATE TABLE IF NOT EXISTS Cycles (
                    StartDate DATETIME PRIMARY KEY,
                    RecordedDate DATETIME
                )",

                @"CREATE TABLE temp_Cycles AS
                SELECT *
                FROM Cycles",

                "DROP TABLE Cycles",

                // We don't care about existing data in AppState because, at
                // this point in the App's life, it'll only hold data that
                // doesn't need to be preserved and is okay if it gets set
                // back to default.
                "DROP TABLE IF EXISTS AppState",
            };

            foreach(var query in queries)
                migrationBuilder.Sql(query);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppState");

            migrationBuilder.DropTable(
                name: "Cycles");
        }
    }
}
