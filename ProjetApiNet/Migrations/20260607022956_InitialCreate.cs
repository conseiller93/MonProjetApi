using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetApiNet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Identifiant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MotDePasseHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZoneMinieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    DistanceDepotZone = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    DistanceAllerRetour = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ToursMaxParJour = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarification = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ChargementsMaxParMois = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneMinieres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupesTransport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SuperviseurGroupeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneMiniereId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupesTransport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupesTransport_Utilisateurs_SuperviseurGroupeId",
                        column: x => x.SuperviseurGroupeId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupesTransport_ZoneMinieres_ZoneMiniereId",
                        column: x => x.ZoneMiniereId,
                        principalTable: "ZoneMinieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Camions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Immatriculation = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Modele = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ChauffeurId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupeTransportId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kilometrage = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Camions_GroupesTransport_GroupeTransportId",
                        column: x => x.GroupeTransportId,
                        principalTable: "GroupesTransport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Camions_Utilisateurs_ChauffeurId",
                        column: x => x.ChauffeurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chargements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CamionId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateHeureDepart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateHeureRetour = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    CarburantCalcule = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chargements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chargements_Camions_CamionId",
                        column: x => x.CamionId,
                        principalTable: "Camions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Camions_ChauffeurId",
                table: "Camions",
                column: "ChauffeurId");

            migrationBuilder.CreateIndex(
                name: "IX_Camions_GroupeTransportId",
                table: "Camions",
                column: "GroupeTransportId");

            migrationBuilder.CreateIndex(
                name: "IX_Camions_Immatriculation",
                table: "Camions",
                column: "Immatriculation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chargements_CamionId",
                table: "Chargements",
                column: "CamionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupesTransport_Nom",
                table: "GroupesTransport",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupesTransport_SuperviseurGroupeId",
                table: "GroupesTransport",
                column: "SuperviseurGroupeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupesTransport_ZoneMiniereId",
                table: "GroupesTransport",
                column: "ZoneMiniereId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Identifiant",
                table: "Utilisateurs",
                column: "Identifiant",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZoneMinieres_Nom",
                table: "ZoneMinieres",
                column: "Nom",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chargements");

            migrationBuilder.DropTable(
                name: "Camions");

            migrationBuilder.DropTable(
                name: "GroupesTransport");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "ZoneMinieres");
        }
    }
}
