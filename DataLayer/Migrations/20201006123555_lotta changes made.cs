using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class lottachangesmade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_PaymentType_PaymentTypeId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Tax_AssessmentNotification_AssessmentNotificationId",
                table: "Tax");

            migrationBuilder.DropForeignKey(
                name: "FK_XMLArchive_AssessmentNotification_AssessmentNotificationId",
                table: "XMLArchive");

            migrationBuilder.DropTable(
                name: "AssessmentNotification");

            migrationBuilder.DropTable(
                name: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_XMLArchive_AssessmentNotificationId",
                table: "XMLArchive");

            migrationBuilder.DropIndex(
                name: "IX_Tax_AssessmentNotificationId",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "AssessmentNotificationId",
                table: "XMLArchive");

            migrationBuilder.DropColumn(
                name: "AssessmentNotificationId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Payment");

            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentId",
                table: "XMLArchive",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.CreateIndex(
                name: "IX_XMLArchive_AssessmentId",
                table: "XMLArchive",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_AssessmentId",
                table: "Tax",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_AssessmentTypeId",
                table: "Assessment",
                column: "AssessmentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tax_Assessment_AssessmentId",
                table: "Tax",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XMLArchive_Assessment_AssessmentId",
                table: "XMLArchive",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tax_Assessment_AssessmentId",
                table: "Tax");

            migrationBuilder.DropForeignKey(
                name: "FK_XMLArchive_Assessment_AssessmentId",
                table: "XMLArchive");

            migrationBuilder.DropTable(
                name: "Assessment");

            migrationBuilder.DropTable(
                name: "AssessmentType");

            migrationBuilder.DropIndex(
                name: "IX_XMLArchive_AssessmentId",
                table: "XMLArchive");

            migrationBuilder.DropIndex(
                name: "IX_Tax_AssessmentId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "XMLArchive");

            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentNotificationId",
                table: "XMLArchive",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentNotificationId",
                table: "Tax",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTypeId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Payment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AssessmentNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankBranchCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarantCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarantName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormMNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADAssessmentDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADAssessmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADAssessmentSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SADYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmountToBePaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentNotification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XMLArchive_AssessmentNotificationId",
                table: "XMLArchive",
                column: "AssessmentNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_AssessmentNotificationId",
                table: "Tax",
                column: "AssessmentNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_PaymentType_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId",
                principalTable: "PaymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tax_AssessmentNotification_AssessmentNotificationId",
                table: "Tax",
                column: "AssessmentNotificationId",
                principalTable: "AssessmentNotification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_XMLArchive_AssessmentNotification_AssessmentNotificationId",
                table: "XMLArchive",
                column: "AssessmentNotificationId",
                principalTable: "AssessmentNotification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
