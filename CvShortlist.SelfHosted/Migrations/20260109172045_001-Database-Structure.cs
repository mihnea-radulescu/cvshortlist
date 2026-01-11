using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvShortlist.SelfHosted.Migrations
{
    /// <inheritdoc />
    public partial class _001DatabaseStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobOpenings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AnalysisLanguage = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Status = table.Column<byte>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateLastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOpenings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateCvs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Sha256FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Analysis = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<byte>(type: "INTEGER", nullable: true),
                    JobOpeningId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCvs_JobOpenings_JobOpeningId",
                        column: x => x.JobOpeningId,
                        principalTable: "JobOpenings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateCvBlobs",
                columns: table => new
                {
                    CandidateCvId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvBlobs", x => x.CandidateCvId);
                    table.ForeignKey(
                        name: "FK_CandidateCvBlobs_CandidateCvs_CandidateCvId",
                        column: x => x.CandidateCvId,
                        principalTable: "CandidateCvs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_JobOpeningId_DateCreated_FileName",
                table: "CandidateCvs",
                columns: new[] { "JobOpeningId", "DateCreated", "FileName" },
                descending: new[] { false, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_JobOpeningId_Rating",
                table: "CandidateCvs",
                columns: new[] { "JobOpeningId", "Rating" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_JobOpeningId_Sha256FileHash",
                table: "CandidateCvs",
                columns: new[] { "JobOpeningId", "Sha256FileHash" });

            migrationBuilder.CreateIndex(
                name: "IX_JobOpenings_Status",
                table: "JobOpenings",
                column: "Status",
                filter: "Status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateCvBlobs");

            migrationBuilder.DropTable(
                name: "CandidateCvs");

            migrationBuilder.DropTable(
                name: "JobOpenings");
        }
    }
}
