using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementWebApp.Data.Migrations
{
    public partial class InitData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "Id", "IsDisabled", "Name" },
                values: new object[] { (short)1, false, "Đồ án cơ sở" });

            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "Id", "IsDisabled", "Name" },
                values: new object[] { (short)2, false, "Đồ án chuyên ngành" });

            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "Id", "IsDisabled", "Name" },
                values: new object[] { (short)3, false, "Đồ án tổng hợp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProjectTypes",
                keyColumn: "Id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "ProjectTypes",
                keyColumn: "Id",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "ProjectTypes",
                keyColumn: "Id",
                keyValue: (short)3);
        }
    }
}
