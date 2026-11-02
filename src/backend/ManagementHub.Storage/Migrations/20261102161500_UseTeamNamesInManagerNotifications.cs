using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <summary>
/// Corrects legacy team-manager assignment messages while preserving notification state and links.
/// </summary>
[DbContext(typeof(ManagementHubDbContext))]
[Migration("20261102161500_UseTeamNamesInManagerNotifications")]
public class UseTeamNamesInManagerNotifications : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(
			"""
			UPDATE notifications
			SET message = (
				SELECT 'You can now manage team ' || teams.name || '.'
				FROM teams
				WHERE notifications.related_entity_id = 'TM_' || CAST(teams.id AS text)
			)
			WHERE type = 3 -- ManagerAssignment
				AND related_entity_type = 'Team'
				AND message = 'You can now manage team ' || related_entity_id || '.'
				AND EXISTS (
					SELECT 1 FROM teams
					WHERE notifications.related_entity_id = 'TM_' || CAST(teams.id AS text)
				);
			""");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Corrected messages cannot be distinguished from notifications originally created with names.
	}
}
