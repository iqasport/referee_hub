using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddUserProfileFields : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateOnly>(
			name: "DateOfBirth",
			table: "users",
			type: "date",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "EmergencyContact",
			table: "users",
			type: "TEXT",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "FoodRestrictions",
			table: "users",
			type: "TEXT",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "MedicalInformation",
			table: "users",
			type: "TEXT",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "DateOfBirth",
			table: "users");

		migrationBuilder.DropColumn(
			name: "EmergencyContact",
			table: "users");

		migrationBuilder.DropColumn(
			name: "FoodRestrictions",
			table: "users");

		migrationBuilder.DropColumn(
			name: "MedicalInformation",
			table: "users");
	}
}
