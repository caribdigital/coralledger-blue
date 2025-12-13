using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace CoralLedger.Blue.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSimplifiedGeometryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "marine_protected_areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WdpaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Boundary = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    BoundarySimplifiedMedium = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    BoundarySimplifiedLow = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    Centroid = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    AreaSquareKm = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    WdpaLastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProtectionLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IslandGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DesignationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ManagingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marine_protected_areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vessels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mmsi = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Imo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CallSign = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GfwVesselId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Flag = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    VesselType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GearType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LengthMeters = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    TonnageGt = table.Column<double>(type: "double precision", precision: 12, scale: 2, nullable: true),
                    YearBuilt = table.Column<int>(type: "integer", nullable: true),
                    LastPositionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    HealthStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepthMeters = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    LengthKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    LastSurveyDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CoralCoverPercentage = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    BleachingPercentage = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reefs_marine_protected_areas_MarineProtectedAreaId",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "vessel_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GfwEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationHours = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    DistanceKm = table.Column<double>(type: "double precision", precision: 10, scale: 3, nullable: true),
                    PortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EncounterVesselId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsInMpa = table.Column<bool>(type: "boolean", nullable: true),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vessel_events_marine_protected_areas_MarineProtectedAreaId",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vessel_events_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vessel_positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpeedKnots = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    Heading = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    IsInMpa = table.Column<bool>(type: "boolean", nullable: true),
                    DistanceFromShoreKm = table.Column<double>(type: "double precision", precision: 10, scale: 3, nullable: true),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vessel_positions_marine_protected_areas_MarineProtectedArea~",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vessel_positions_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bleaching_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    AlertLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeaSurfaceTemperature = table.Column<double>(type: "double precision", precision: 6, scale: 3, nullable: false),
                    SstAnomaly = table.Column<double>(type: "double precision", precision: 6, scale: 3, nullable: false),
                    HotSpot = table.Column<double>(type: "double precision", precision: 6, scale: 3, nullable: true),
                    DegreeHeatingWeek = table.Column<double>(type: "double precision", precision: 8, scale: 3, nullable: false),
                    MarineProtectedAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReefId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bleaching_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bleaching_alerts_marine_protected_areas_MarineProtectedArea~",
                        column: x => x.MarineProtectedAreaId,
                        principalTable: "marine_protected_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bleaching_alerts_reefs_ReefId",
                        column: x => x.ReefId,
                        principalTable: "reefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_AlertLevel",
                table: "bleaching_alerts",
                column: "AlertLevel");

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_Date",
                table: "bleaching_alerts",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_Date_AlertLevel",
                table: "bleaching_alerts",
                columns: new[] { "Date", "AlertLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_DegreeHeatingWeek",
                table: "bleaching_alerts",
                column: "DegreeHeatingWeek");

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_Location",
                table: "bleaching_alerts",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_MarineProtectedAreaId_Date",
                table: "bleaching_alerts",
                columns: new[] { "MarineProtectedAreaId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_bleaching_alerts_ReefId",
                table: "bleaching_alerts",
                column: "ReefId");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_Boundary",
                table: "marine_protected_areas",
                column: "Boundary")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_Centroid",
                table: "marine_protected_areas",
                column: "Centroid")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_IslandGroup",
                table: "marine_protected_areas",
                column: "IslandGroup");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_Name",
                table: "marine_protected_areas",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_ProtectionLevel",
                table: "marine_protected_areas",
                column: "ProtectionLevel");

            migrationBuilder.CreateIndex(
                name: "IX_marine_protected_areas_WdpaId",
                table: "marine_protected_areas",
                column: "WdpaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reefs_HealthStatus",
                table: "reefs",
                column: "HealthStatus");

            migrationBuilder.CreateIndex(
                name: "IX_reefs_Location",
                table: "reefs",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_reefs_MarineProtectedAreaId",
                table: "reefs",
                column: "MarineProtectedAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_reefs_Name",
                table: "reefs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_EventType",
                table: "vessel_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_GfwEventId",
                table: "vessel_events",
                column: "GfwEventId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_IsInMpa",
                table: "vessel_events",
                column: "IsInMpa");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_Location",
                table: "vessel_events",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_MarineProtectedAreaId",
                table: "vessel_events",
                column: "MarineProtectedAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_StartTime",
                table: "vessel_events",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_VesselId",
                table: "vessel_events",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_events_VesselId_EventType_StartTime",
                table: "vessel_events",
                columns: new[] { "VesselId", "EventType", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_IsInMpa",
                table: "vessel_positions",
                column: "IsInMpa");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_Location",
                table: "vessel_positions",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_MarineProtectedAreaId",
                table: "vessel_positions",
                column: "MarineProtectedAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_Timestamp",
                table: "vessel_positions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_VesselId",
                table: "vessel_positions",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_positions_VesselId_Timestamp",
                table: "vessel_positions",
                columns: new[] { "VesselId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Flag",
                table: "vessels",
                column: "Flag");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_GfwVesselId",
                table: "vessels",
                column: "GfwVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Imo",
                table: "vessels",
                column: "Imo");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_IsActive",
                table: "vessels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Mmsi",
                table: "vessels",
                column: "Mmsi");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Name",
                table: "vessels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_VesselType",
                table: "vessels",
                column: "VesselType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bleaching_alerts");

            migrationBuilder.DropTable(
                name: "vessel_events");

            migrationBuilder.DropTable(
                name: "vessel_positions");

            migrationBuilder.DropTable(
                name: "reefs");

            migrationBuilder.DropTable(
                name: "vessels");

            migrationBuilder.DropTable(
                name: "marine_protected_areas");
        }
    }
}
