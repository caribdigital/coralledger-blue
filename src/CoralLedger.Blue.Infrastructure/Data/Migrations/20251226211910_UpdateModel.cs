using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralLedger.Blue.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nlq_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalQuery = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InterpretedAs = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GeneratedSql = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Persona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QueryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataSourcesUsed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiredDisambiguation = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityRestrictionApplied = table.Column<bool>(type: "boolean", nullable: false),
                    UserIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nlq_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "species_misidentification_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncorrectScientificName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CorrectedScientificName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CorrectedSpeciesId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReporterEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReporterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Expertise = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_species_misidentification_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_species_misidentification_reports_bahamian_species_Correcte~",
                        column: x => x.CorrectedSpeciesId,
                        principalTable: "bahamian_species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_species_misidentification_reports_species_observations_Spec~",
                        column: x => x.SpeciesObservationId,
                        principalTable: "species_observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nlq_audit_logs_Persona",
                table: "nlq_audit_logs",
                column: "Persona");

            migrationBuilder.CreateIndex(
                name: "IX_nlq_audit_logs_QueryTime",
                table: "nlq_audit_logs",
                column: "QueryTime");

            migrationBuilder.CreateIndex(
                name: "IX_nlq_audit_logs_SecurityRestrictionApplied",
                table: "nlq_audit_logs",
                column: "SecurityRestrictionApplied");

            migrationBuilder.CreateIndex(
                name: "IX_nlq_audit_logs_Status",
                table: "nlq_audit_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_species_misidentification_reports_CorrectedSpeciesId",
                table: "species_misidentification_reports",
                column: "CorrectedSpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_species_misidentification_reports_Expertise",
                table: "species_misidentification_reports",
                column: "Expertise");

            migrationBuilder.CreateIndex(
                name: "IX_species_misidentification_reports_ReportedAt",
                table: "species_misidentification_reports",
                column: "ReportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_species_misidentification_reports_SpeciesObservationId",
                table: "species_misidentification_reports",
                column: "SpeciesObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_species_misidentification_reports_Status",
                table: "species_misidentification_reports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nlq_audit_logs");

            migrationBuilder.DropTable(
                name: "species_misidentification_reports");
        }
    }
}
