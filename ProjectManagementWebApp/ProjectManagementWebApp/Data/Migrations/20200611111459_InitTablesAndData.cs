using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementWebApp.Data.Migrations
{
    public partial class InitTablesAndData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TableName = table.Column<string>(maxLength: 256, nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    KeyValues = table.Column<string>(nullable: true),
                    OldValues = table.Column<string>(nullable: true),
                    NewValues = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lecturers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    LecturerCode = table.Column<string>(maxLength: 256, nullable: true),
                    IsManager = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lecturers_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypes",
                columns: table => new
                {
                    Id = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(10)", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    StudentCode = table.Column<string>(maxLength: 256, nullable: true),
                    ClassName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectTypeId = table.Column<short>(nullable: false),
                    FacultyId = table.Column<short>(nullable: false),
                    SemesterId = table.Column<short>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    UniqueId = table.Column<string>(maxLength: 450, nullable: true),
                    Note = table.Column<string>(maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_ProjectTypes_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "ProjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLecturers",
                columns: table => new
                {
                    ProjectId = table.Column<int>(nullable: false),
                    LecturerId = table.Column<string>(nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLecturers", x => new { x.ProjectId, x.LecturerId });
                    table.ForeignKey(
                        name: "FK_ProjectLecturers_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectLecturers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    ProjectId = table.Column<int>(nullable: false),
                    StudentId = table.Column<string>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Name" },
                values: new object[] { (short)1, "Công nghệ thông tin" });

            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "Id", "IsDisabled", "Name" },
                values: new object[,]
                {
                    { (short)1, false, "Đồ án cơ sở" },
                    { (short)2, false, "Đồ án chuyên ngành" },
                    { (short)3, false, "Đồ án tổng hợp" }
                });

            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "Id", "EndedDate", "Name", "StartedDate" },
                values: new object[,]
                {
                    { (short)1, new DateTime(2020, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2019-1", new DateTime(2019, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { (short)2, new DateTime(2020, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2019-2", new DateTime(2020, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLecturers_LecturerId",
                table: "ProjectLecturers",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_StudentId",
                table: "ProjectMembers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FacultyId",
                table: "Projects",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectTypeId",
                table: "Projects",
                column: "ProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SemesterId",
                table: "Projects",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UniqueId",
                table: "Projects",
                column: "UniqueId",
                unique: true,
                filter: "[UniqueId] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_StartedDate",
                table: "Semesters",
                column: "StartedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "ProjectLecturers");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "ProjectScheduleReportFiles");

            migrationBuilder.DropTable(
                name: "Lecturers");

            migrationBuilder.DropTable(
                name: "ProjectScheduleReports");

            migrationBuilder.DropTable(
                name: "ProjectSchedules");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Faculties");

            migrationBuilder.DropTable(
                name: "ProjectTypes");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");
        }
    }
}
