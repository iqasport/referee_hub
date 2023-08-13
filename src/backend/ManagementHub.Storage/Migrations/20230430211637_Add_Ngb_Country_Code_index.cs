using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class Add_Ngb_Country_Code_index : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "countrycode",
			table: "national_governing_bodies",
			type: "varchar(3)",
			nullable: true);

		// copy data from acronym into the new column
		migrationBuilder.Sql("UPDATE national_governing_bodies SET countrycode = acronym;");

		migrationBuilder.AlterColumn<string>(
			name: "countrycode",
			table: "national_governing_bodies",
			type: "varchar(3)",
			nullable: false);

		migrationBuilder.CreateIndex(
			name: "index_national_governing_bodies_on_country_code",
			table: "national_governing_bodies",
			column: "countrycode",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "index_national_governing_bodies_on_country_code",
			table: "national_governing_bodies");

		migrationBuilder.DropColumn(
			name: "countrycode",
			table: "national_governing_bodies");
	}
}
