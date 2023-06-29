﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employees.Migrations
{
    /// <inheritdoc />
    public partial class OneTableForAllergiesAndDietaryPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDefaultAllergies");

            migrationBuilder.DropTable(
                name: "EmployeeDietaryPreferences");

            migrationBuilder.DropTable(
                name: "EmployeeOtherAllergies");

            migrationBuilder.CreateTable(
                name: "EmployeeAllergiesAndDietaryPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultAllergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtherAllergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DietaryPreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAllergiesAndDietaryPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAllergiesAndDietaryPreferences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAllergiesAndDietaryPreferences_EmployeeId",
                table: "EmployeeAllergiesAndDietaryPreferences",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAllergiesAndDietaryPreferences");

            migrationBuilder.CreateTable(
                name: "EmployeeDefaultAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultAllergy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDefaultAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDefaultAllergies_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDietaryPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DietaryPreference = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDietaryPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDietaryPreferences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOtherAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtherAllergy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOtherAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOtherAllergies_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDefaultAllergies_EmployeeId",
                table: "EmployeeDefaultAllergies",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDietaryPreferences_EmployeeId",
                table: "EmployeeDietaryPreferences",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOtherAllergies_EmployeeId",
                table: "EmployeeOtherAllergies",
                column: "EmployeeId");
        }
    }
}
