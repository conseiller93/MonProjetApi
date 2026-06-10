using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetApiNet.Migrations
{
    /// <inheritdoc />
    public partial class AjoutColonnesChargement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CarburantCalcule",
                table: "Chargements",
                newName: "CarburantTotalLitres");

            migrationBuilder.AlterColumn<decimal>(
                name: "Tarification",
                table: "ZoneMinieres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceDepotZone",
                table: "ZoneMinieres",
                type: "decimal(18,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceAllerRetour",
                table: "ZoneMinieres",
                type: "decimal(18,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaireMensuelGNF",
                table: "Utilisateurs",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EstCloture",
                table: "Chargements",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimeChauffeurGNF",
                table: "Chargements",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimeSupervGenGNF",
                table: "Chargements",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimeSuperviseurGroupeGNF",
                table: "Chargements",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimeSuperviseurZoneGNF",
                table: "Chargements",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifGNF",
                table: "Chargements",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ZoneMiniereId",
                table: "Chargements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Chargements_ZoneMiniereId",
                table: "Chargements",
                column: "ZoneMiniereId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chargements_ZoneMinieres_ZoneMiniereId",
                table: "Chargements",
                column: "ZoneMiniereId",
                principalTable: "ZoneMinieres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chargements_ZoneMinieres_ZoneMiniereId",
                table: "Chargements");

            migrationBuilder.DropIndex(
                name: "IX_Chargements_ZoneMiniereId",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "SalaireMensuelGNF",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "EstCloture",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "PrimeChauffeurGNF",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "PrimeSupervGenGNF",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "PrimeSuperviseurGroupeGNF",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "PrimeSuperviseurZoneGNF",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "TarifGNF",
                table: "Chargements");

            migrationBuilder.DropColumn(
                name: "ZoneMiniereId",
                table: "Chargements");

            migrationBuilder.RenameColumn(
                name: "CarburantTotalLitres",
                table: "Chargements",
                newName: "CarburantCalcule");

            migrationBuilder.AlterColumn<decimal>(
                name: "Tarification",
                table: "ZoneMinieres",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceDepotZone",
                table: "ZoneMinieres",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceAllerRetour",
                table: "ZoneMinieres",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
