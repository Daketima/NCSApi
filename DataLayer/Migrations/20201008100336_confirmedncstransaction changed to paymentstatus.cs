using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class confirmedncstransactionchangedtopaymentstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "confirmTransaction");

            migrationBuilder.AddColumn<int>(
                name: "PaymentTypeId",
                table: "Payment",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Year = table.Column<string>(nullable: true),
                    CustomsCode = table.Column<string>(nullable: true),
                    CompanyCode = table.Column<string>(nullable: true),
                    DeclarantCode = table.Column<string>(nullable: true),
                    AssessmentSerial = table.Column<string>(nullable: true),
                    AssessmentNumber = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    ErrorCode = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    PaymentLogId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentStatus_Payment_PaymentLogId",
                        column: x => x.PaymentLogId,
                        principalTable: "Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AssessmentType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Excise" });

            migrationBuilder.InsertData(
                table: "AssessmentType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "SD" });

            migrationBuilder.InsertData(
                table: "AssessmentType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "SGD" });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatus_PaymentLogId",
                table: "PaymentStatus",
                column: "PaymentLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_AssessmentType_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId",
                principalTable: "AssessmentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_AssessmentType_PaymentTypeId",
                table: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentStatus");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment");

            migrationBuilder.DeleteData(
                table: "AssessmentType",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AssessmentType",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AssessmentType",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "Payment");

            migrationBuilder.CreateTable(
                name: "confirmTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeclarantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADAssessmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADAssessmentSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confirmTransaction", x => x.Id);
                });
        }
    }
}
