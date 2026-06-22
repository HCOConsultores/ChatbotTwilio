using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatBotTwilio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLogbookAndState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentContract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentProjectId = table.Column<int>(type: "int", nullable: true),
                    CurrentProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentNoteType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Supervisor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcorta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastInteraction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectPhotoInfoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TempDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logbook",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_project = table.Column<int>(type: "int", nullable: true),
                    id_reporte = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: true),
                    timexnote = table.Column<TimeSpan>(type: "time(7)", nullable: true),
                    typenote = table.Column<string>(type: "varchar(25)", nullable: false),
                    supervisor = table.Column<string>(type: "varchar(250)", nullable: false),
                    image = table.Column<string>(type: "varchar(320)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    imageazure = table.Column<string>(type: "varchar(320)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(24)", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<short>(type: "smallint", nullable: true),
                    id_ot = table.Column<int>(type: "int", nullable: true),
                    id_resource = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(12,3)", nullable: true),
                    start = table.Column<TimeSpan>(type: "time(7)", nullable: true),
                    end = table.Column<TimeSpan>(type: "time(7)", nullable: true),
                    position = table.Column<string>(type: "varchar(50)", nullable: true),
                    imageReal = table.Column<string>(type: "varchar(320)", nullable: true),
                    chatid = table.Column<string>(type: "nvarchar(24)", nullable: true),
                    cuadrilla = table.Column<string>(type: "varchar(20)", nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    report_section_id = table.Column<int>(type: "int", nullable: true),
                    task_reference = table.Column<string>(type: "varchar(50)", nullable: true),
                    notes = table.Column<string>(type: "varchar(500)", nullable: true),
                    fuel_quantity = table.Column<int>(type: "int", nullable: true),
                    report_date = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logbook", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oilfields",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_contract = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "varchar(100)", nullable: false),
                    direccion = table.Column<string>(type: "varchar(100)", nullable: false),
                    coordinates = table.Column<string>(type: "varchar(100)", nullable: false),
                    active = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oilfields", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(500)", nullable: false),
                    nameshort = table.Column<string>(type: "varchar(30)", nullable: false),
                    rfc = table.Column<string>(type: "varchar(13)", nullable: false),
                    address = table.Column<string>(type: "varchar(100)", nullable: false),
                    id_state = table.Column<int>(type: "int", nullable: true),
                    phone = table.Column<string>(type: "varchar(10)", nullable: false),
                    consortium = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    picture = table.Column<string>(type: "varchar(250)", nullable: false),
                    active = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    consecutive = table.Column<int>(type: "int", nullable: true),
                    speciality = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    contract = table.Column<string>(type: "varchar(20)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    descripsmall = table.Column<string>(type: "varchar(150)", nullable: false),
                    id_provider = table.Column<int>(type: "int", nullable: true),
                    statecontract = table.Column<string>(type: "varchar(40)", nullable: true),
                    datestar = table.Column<DateTime>(type: "date", nullable: true),
                    dateend = table.Column<DateTime>(type: "date", nullable: true),
                    term = table.Column<int>(type: "int", nullable: false),
                    amountmx = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    amountdll = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    resident = table.Column<string>(type: "varchar(150)", nullable: true),
                    supervisor = table.Column<string>(type: "varchar(150)", nullable: true),
                    active = table.Column<short>(type: "smallint", nullable: true),
                    id_provider1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracts_providers_id_provider",
                        column: x => x.id_provider,
                        principalTable: "providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idConsecutivo = table.Column<int>(type: "int", nullable: true),
                    number = table.Column<string>(type: "varchar(20)", nullable: false),
                    id_contrato = table.Column<int>(type: "int", nullable: true),
                    id_oilfield = table.Column<int>(type: "int", nullable: true),
                    id_active = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    year = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    diameter = table.Column<string>(type: "nchar(10)", nullable: false),
                    length = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<byte>(type: "tinyint", nullable: false),
                    budgetmanagement = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    linerigth = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    government = table.Column<string>(type: "varchar(2)", nullable: false),
                    receivedenginnering = table.Column<string>(type: "varchar(2)", nullable: false),
                    datedelivery = table.Column<DateTime>(type: "date", nullable: true),
                    supplypipe = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    request = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    programstart = table.Column<DateTime>(type: "date", nullable: true),
                    programend = table.Column<DateTime>(type: "date", nullable: true),
                    realpronosticLPO = table.Column<DateTime>(type: "date", nullable: true),
                    realpronosticTTT = table.Column<DateTime>(type: "date", nullable: true),
                    classification = table.Column<string>(type: "varchar(10)", nullable: false),
                    state = table.Column<string>(type: "varchar(10)", nullable: false),
                    typeconstruction = table.Column<string>(type: "varchar(25)", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(80)", nullable: false),
                    active = table.Column<short>(type: "smallint", nullable: true),
                    id_contract = table.Column<int>(type: "int", nullable: false),
                    id_oilfield1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_contracts_id_contrato",
                        column: x => x.id_contrato,
                        principalTable: "contracts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_projects_oilfields_id_oilfield",
                        column: x => x.id_oilfield,
                        principalTable: "oilfields",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_id_provider",
                table: "contracts",
                column: "id_provider");

            migrationBuilder.CreateIndex(
                name: "IX_projects_id_contrato",
                table: "projects",
                column: "id_contrato");

            migrationBuilder.CreateIndex(
                name: "IX_projects_id_oilfield",
                table: "projects",
                column: "id_oilfield");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationStates");

            migrationBuilder.DropTable(
                name: "logbook");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "oilfields");

            migrationBuilder.DropTable(
                name: "providers");
        }
    }
}
