using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementWebApp.Data.Migrations
{
    public partial class ProjectSchedulesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Rating = table.Column<float>(nullable: true),
                    StartedDate = table.Column<DateTime>(nullable: true),
                    ExpiredDate = table.Column<DateTime>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSchedules_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectScheduleReports",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectScheduleId = table.Column<int>(nullable: false),
                    StudentId = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScheduleReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScheduleReports_ProjectSchedules_ProjectScheduleId",
                        column: x => x.ProjectScheduleId,
                        principalTable: "ProjectSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectScheduleReports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectScheduleReportFiles",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    ProjectScheduleReportId = table.Column<long>(nullable: false),
                    FileName = table.Column<string>(maxLength: 256, nullable: true),
                    Path = table.Column<string>(nullable: true),
                    UploadedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScheduleReportFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScheduleReportFiles_ProjectScheduleReports_ProjectScheduleReportId",
                        column: x => x.ProjectScheduleReportId,
                        principalTable: "ProjectScheduleReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScheduleReportFiles_ProjectScheduleReportId",
                table: "ProjectScheduleReportFiles",
                column: "ProjectScheduleReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScheduleReports_ProjectScheduleId",
                table: "ProjectScheduleReports",
                column: "ProjectScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScheduleReports_StudentId",
                table: "ProjectScheduleReports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSchedules_ProjectId",
                table: "ProjectSchedules",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectScheduleReportFiles");

            migrationBuilder.DropTable(
                name: "ProjectScheduleReports");

            migrationBuilder.DropTable(
                name: "ProjectSchedules");
        }
    }
}
