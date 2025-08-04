using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkIntakeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusinessVerticals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9567), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9569) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9789), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9790) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9809), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9809) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9897), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9897) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9913), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9913) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9927), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9927) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9945), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9945) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9960), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9960) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9973), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9974) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9987), new DateTime(2025, 8, 4, 2, 54, 32, 113, DateTimeKind.Utc).AddTicks(9987) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(2), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(3) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(15), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(15) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(28), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(28) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(40), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(41) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(54), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(54) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(68), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(68) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(81), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(81) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(115), new DateTime(2025, 8, 4, 2, 54, 32, 114, DateTimeKind.Utc).AddTicks(115) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusinessVerticals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5155), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5157) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5374), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5375) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5399), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5399) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5417), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5417) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5434), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5434) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5451), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5451) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5471), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5471) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5487), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5487) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5503), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5503) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5519), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5520) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5538), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5539) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5555), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5555) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5588), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5588) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5606), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5606) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5677), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5677) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5697), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5697) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5714), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5715) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5730), new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5731) });
        }
    }
}
