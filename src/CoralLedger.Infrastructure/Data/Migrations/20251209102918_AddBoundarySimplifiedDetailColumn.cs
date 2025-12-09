using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace CoralLedger.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBoundarySimplifiedDetailColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "BoundarySimplifiedDetail",
                table: "marine_protected_areas",
                type: "geometry(Geometry, 4326)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Conditions = table.Column<string>(type: "jsonb", nullable: false),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    NotificationChannels = table.Column<int>(type: "integer", nullable: false),
                    NotificationEmails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CooldownPeriod = table.Column<TimeSpan>(type: "interval", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRules_marine_protected_areas_MarineProtectedAreaId",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bahamian_species",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScientificName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CommonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConservationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsInvasive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdentificationTips = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Habitat = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TypicalDepthMinM = table.Column<int>(type: "integer", nullable: true),
                    TypicalDepthMaxM = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bahamian_species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: true),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: true),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerts_marine_protected_areas_MarineProtectedAreaId",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "species_observations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BahamianSpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    AiConfidenceScore = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    RequiresExpertVerification = table.Column<bool>(type: "boolean", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdentifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_species_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_species_observations_bahamian_species_BahamianSpeciesId",
                        column: x => x.BahamianSpeciesId,
                        principalTable: "bahamian_species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_species_observations_citizen_observations_CitizenObservatio~",
                        column: x => x.CitizenObservationId,
                        principalTable: "citizen_observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_IsActive",
                table: "AlertRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_MarineProtectedAreaId",
                table: "AlertRules",
                column: "MarineProtectedAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_Type",
                table: "AlertRules",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertRuleId",
                table: "Alerts",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAt",
                table: "Alerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_IsAcknowledged",
                table: "Alerts",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_MarineProtectedAreaId",
                table: "Alerts",
                column: "MarineProtectedAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Severity",
                table: "Alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Type",
                table: "Alerts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Type_CreatedAt",
                table: "Alerts",
                columns: new[] { "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_VesselId",
                table: "Alerts",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_Category",
                table: "bahamian_species",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_CommonName",
                table: "bahamian_species",
                column: "CommonName");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_ConservationStatus",
                table: "bahamian_species",
                column: "ConservationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_IsInvasive",
                table: "bahamian_species",
                column: "IsInvasive");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_LocalName",
                table: "bahamian_species",
                column: "LocalName");

            migrationBuilder.CreateIndex(
                name: "IX_bahamian_species_ScientificName",
                table: "bahamian_species",
                column: "ScientificName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_species_observations_BahamianSpeciesId",
                table: "species_observations",
                column: "BahamianSpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_species_observations_CitizenObservationId_BahamianSpeciesId",
                table: "species_observations",
                columns: new[] { "CitizenObservationId", "BahamianSpeciesId" });

            migrationBuilder.CreateIndex(
                name: "IX_species_observations_IdentifiedAt",
                table: "species_observations",
                column: "IdentifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_species_observations_IsAiGenerated",
                table: "species_observations",
                column: "IsAiGenerated");

            migrationBuilder.CreateIndex(
                name: "IX_species_observations_RequiresExpertVerification",
                table: "species_observations",
                column: "RequiresExpertVerification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "species_observations");

            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "bahamian_species");

            migrationBuilder.DropColumn(
                name: "BoundarySimplifiedDetail",
                table: "marine_protected_areas");
        }
    }
}
