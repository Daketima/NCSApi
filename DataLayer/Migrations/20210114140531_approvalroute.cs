using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class approvalroute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InitiatedByBranchCode",
                table: "Payment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitiatedBy",
                table: "Assessment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitiatedByBranchCode",
                table: "Assessment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatedByBranchCode",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "InitiatedBy",
                table: "Assessment");

            migrationBuilder.DropColumn(
                name: "InitiatedByBranchCode",
                table: "Assessment");
        }
    }
}
