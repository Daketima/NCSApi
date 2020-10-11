using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class initialmigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SADYear = table.Column<string>(nullable: true),
                    CustomsCode = table.Column<string>(nullable: true),
                    DeclarantCode = table.Column<string>(nullable: true),
                    DeclarantName = table.Column<string>(nullable: true),
                    SADAssessmentSerial = table.Column<string>(nullable: true),
                    SADAssessmentNumber = table.Column<string>(nullable: true),
                    SADAssessmentDate = table.Column<string>(nullable: true),
                    CompanyCode = table.Column<string>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    BankCode = table.Column<string>(nullable: true),
                    BankBranchCode = table.Column<string>(nullable: true),
                    FormMNumber = table.Column<string>(nullable: true),
                    TotalAmountToBePaid = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentNotification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "confirmTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SADYear = table.Column<string>(nullable: true),
                    CustomsCode = table.Column<string>(nullable: true),
                    DeclarantCode = table.Column<string>(nullable: true),
                    SADAssessmentSerial = table.Column<string>(nullable: true),
                    SADAssessmentNumber = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confirmTransaction", x => x.Id);
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
                name: "PaymentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tax",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TaxCode = table.Column<string>(nullable: true),
                    TaxAmount = table.Column<string>(nullable: true),
                    AssessmentId = table.Column<Guid>(nullable: false),
                    AssessmentNotificationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tax_AssessmentNotification_AssessmentNotificationId",
                        column: x => x.AssessmentNotificationId,
                        principalTable: "AssessmentNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "XMLArchive",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RawXML = table.Column<string>(nullable: true),
                    AssessmentNotificationId = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XMLArchive_AssessmentNotification_AssessmentNotificationId",
                        column: x => x.AssessmentNotificationId,
                        principalTable: "AssessmentNotification",
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
                    TypeId = table.Column<int>(nullable: false),
                    PaymentTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_PaymentType_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_LOV_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LOV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_StatusId",
                table: "Payment",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_AssessmentNotificationId",
                table: "Tax",
                column: "AssessmentNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_XMLArchive_AssessmentNotificationId",
                table: "XMLArchive",
                column: "AssessmentNotificationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "confirmTransaction");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Tax");

            migrationBuilder.DropTable(
                name: "XMLArchive");

            migrationBuilder.DropTable(
                name: "PaymentType");

            migrationBuilder.DropTable(
                name: "LOV");

            migrationBuilder.DropTable(
                name: "AssessmentNotification");
        }
    }
}
