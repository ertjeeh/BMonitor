using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMonitor.DAL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Monitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateIntervalInMs = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusResults",
                columns: table => new
                {
                    StatusResultId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusResults", x => x.StatusResultId);
                });

            migrationBuilder.CreateTable(
                name: "HtmlCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HtmlCards_Cards_Id",
                        column: x => x.Id,
                        principalTable: "Cards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CardInstance",
                columns: table => new
                {
                    CardsId = table.Column<int>(type: "int", nullable: false),
                    InstancesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardInstance", x => new { x.CardsId, x.InstancesId });
                    table.ForeignKey(
                        name: "FK_CardInstance_Cards_CardsId",
                        column: x => x.CardsId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardInstance_Instances_InstancesId",
                        column: x => x.InstancesId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderMonitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderMonitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderMonitors_Monitors_Id",
                        column: x => x.Id,
                        principalTable: "Monitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HttpMonitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedStatusCode = table.Column<int>(type: "int", nullable: false),
                    TimeoutInMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpMonitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpMonitors_Monitors_Id",
                        column: x => x.Id,
                        principalTable: "Monitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PingMonitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeoutInMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PingMonitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PingMonitors_Monitors_Id",
                        column: x => x.Id,
                        principalTable: "Monitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SqlMonitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlMonitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlMonitors_Monitors_Id",
                        column: x => x.Id,
                        principalTable: "Monitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MonitorResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitorId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusResultId = table.Column<int>(type: "int", nullable: false),
                    IntResult = table.Column<int>(type: "int", nullable: true),
                    StringResult = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitorResults_Monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitorResults_StatusResults_StatusResultId",
                        column: x => x.StatusResultId,
                        principalTable: "StatusResults",
                        principalColumn: "StatusResultId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StatusResults",
                columns: new[] { "StatusResultId", "Name" },
                values: new object[] { -1, "Failed" });

            migrationBuilder.InsertData(
                table: "StatusResults",
                columns: new[] { "StatusResultId", "Name" },
                values: new object[] { 0, "Succeeded" });

            migrationBuilder.InsertData(
                table: "StatusResults",
                columns: new[] { "StatusResultId", "Name" },
                values: new object[] { 1, "Unknown" });

            migrationBuilder.CreateIndex(
                name: "IX_CardInstance_InstancesId",
                table: "CardInstance",
                column: "InstancesId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_MonitorId",
                table: "MonitorResults",
                column: "MonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_StatusResultId",
                table: "MonitorResults",
                column: "StatusResultId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardInstance");

            migrationBuilder.DropTable(
                name: "FolderMonitors");

            migrationBuilder.DropTable(
                name: "HtmlCards");

            migrationBuilder.DropTable(
                name: "HttpMonitors");

            migrationBuilder.DropTable(
                name: "MonitorResults");

            migrationBuilder.DropTable(
                name: "PingMonitors");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "SqlMonitors");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "StatusResults");

            migrationBuilder.DropTable(
                name: "Monitors");
        }
    }
}
