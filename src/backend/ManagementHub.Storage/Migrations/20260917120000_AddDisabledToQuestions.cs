using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds the Disabled column to the questions table, allowing individual questions to be
/// excluded from new test attempts while preserving historical referee answers for past attempts.
/// </remarks>
public partial class AddDisabledToQuestions : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
				name: "disabled",
				table: "questions",
				type: "boolean",
				nullable: false,
				defaultValue: false);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
				name: "disabled",
				table: "questions");
	}
}
