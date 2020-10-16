using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class initialcreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LOV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOV", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Year = table.Column<string>(nullable: true),
                    CustomsCode = table.Column<string>(nullable: true),
                    DeclarantCode = table.Column<string>(nullable: true),
                    DeclarantName = table.Column<string>(nullable: true),
                    AssessmentSerial = table.Column<string>(nullable: true),
                    AssessmentNumber = table.Column<string>(nullable: true),
                    AssessmentDate = table.Column<string>(nullable: true),
                    PassportNumber = table.Column<string>(nullable: true),
                    CompanyCode = table.Column<string>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    BankCode = table.Column<string>(nullable: true),
                    BankBranchCode = table.Column<string>(nullable: true),
                    FormMNumber = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    TotalAmountToBePaid = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    AssessmentTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessment_AssessmentType_AssessmentTypeId",
                        column: x => x.AssessmentTypeId,
                        principalTable: "AssessmentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CustomerAccount = table.Column<string>(nullable: true),
                    Amount = table.Column<string>(nullable: true),
                    StatusId = table.Column<int>(nullable: false),
                    PaymentReference = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    AssessmentId = table.Column<string>(nullable: true),
                    TransactionStatusId = table.Column<int>(nullable: false),
                    PaymentTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_AssessmentType_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "AssessmentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_LOV_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LOV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tax",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TaxCode = table.Column<string>(nullable: true),
                    TaxAmount = table.Column<string>(nullable: true),
                    AssessmentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tax_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XMLArchive",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RawXML = table.Column<string>(nullable: true),
                    AssessmentId = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XMLArchive_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_AssessmentTypeId",
                table: "Assessment",
                column: "AssessmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_StatusId",
                table: "Payment",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatus_PaymentLogId",
                table: "PaymentStatus",
                column: "PaymentLogId");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_AssessmentId",
                table: "Tax",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_XMLArchive_AssessmentId",
                table: "XMLArchive",
                column: "AssessmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentStatus");

            migrationBuilder.DropTable(
                name: "Tax");

            migrationBuilder.DropTable(
                name: "XMLArchive");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Assessment");

            migrationBuilder.DropTable(
                name: "LOV");

            migrationBuilder.DropTable(
                name: "AssessmentType");
        }
    }
}
