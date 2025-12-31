using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddPhase4TournamentRosters : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
			name: "sign_in_count",
			table: "users",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<bool>(
			name: "show_pronouns",
			table: "users",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "boolean",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<string>(
			name: "last_sign_in_ip",
			table: "users",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "inet",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "language_id",
			table: "users",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "invited_by_id",
			table: "users",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "invitations_count",
			table: "users",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "invitation_limit",
			table: "users",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "failed_attempts",
			table: "users",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<bool>(
			name: "export_name",
			table: "users",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "true",
			oldClrType: typeof(bool),
			oldType: "boolean",
			oldNullable: true,
			oldDefaultValueSql: "true");

		migrationBuilder.AlterColumn<string>(
			name: "current_sign_in_ip",
			table: "users",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "inet",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "bio",
			table: "users",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "admin",
			table: "users",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "boolean",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "users",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "attribute_value",
			table: "user_attributes",
			type: "TEXT",
			maxLength: 4096,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "character varying(4096)",
			oldMaxLength: 4096);

		migrationBuilder.AlterColumn<string>(
			name: "key",
			table: "user_attributes",
			type: "TEXT",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "character varying(128)",
			oldMaxLength: 128);

		migrationBuilder.AlterColumn<string>(
			name: "prefix",
			table: "user_attributes",
			type: "TEXT",
			maxLength: 16,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "character varying(16)",
			oldMaxLength: 16);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "user_attributes",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<int>(
			name: "type",
			table: "tournaments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<bool>(
			name: "is_private",
			table: "tournaments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "boolean");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournaments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_team_participants",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "tournament_team_participants",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_team_participants",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "tournament_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "added_by_user_id",
			table: "tournament_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "tournament_manager_approval",
			table: "tournament_invites",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_invites",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<int>(
			name: "participant_approval",
			table: "tournament_invites",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "initiator_user_id",
			table: "tournament_invites",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_invites",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "time_limit",
			table: "tests",
			type: "INTEGER",
			nullable: false,
			defaultValueSql: "18",
			oldClrType: typeof(int),
			oldType: "integer",
			oldDefaultValueSql: "18");

		migrationBuilder.AlterColumn<int>(
			name: "testable_question_count",
			table: "tests",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<bool>(
			name: "recertification",
			table: "tests",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "boolean",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<string>(
			name: "positive_feedback",
			table: "tests",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "new_language_id",
			table: "tests",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "negative_feedback",
			table: "tests",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "minimum_pass_percentage",
			table: "tests",
			type: "INTEGER",
			nullable: false,
			defaultValueSql: "80",
			oldClrType: typeof(int),
			oldType: "integer",
			oldDefaultValueSql: "80");

		migrationBuilder.AlterColumn<int>(
			name: "level",
			table: "tests",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "tests",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "tests",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "active",
			table: "tests",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "boolean");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tests",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<TimeOnly>(
			name: "time_started",
			table: "test_results",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(TimeOnly),
			oldType: "time without time zone",
			oldNullable: true);

		migrationBuilder.AlterColumn<TimeOnly>(
			name: "time_finished",
			table: "test_results",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(TimeOnly),
			oldType: "time without time zone",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "test_level",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "test_results",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<int>(
			name: "points_scored",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "points_available",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "percentage",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "passed",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(bool),
			oldType: "boolean",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "minimum_pass_percentage",
			table: "test_results",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "test_results",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "test_level",
			table: "test_attempts",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "test_attempts",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "test_attempts",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "test_attempts",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "status",
			table: "teams",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "teams",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "group_affiliation",
			table: "teams",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "teams",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "team_status_changesets",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "team_status_changesets",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "team_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "team_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "team_managers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "ownable_id",
			table: "social_accounts",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "account_type",
			table: "social_accounts",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "social_accounts",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "roles",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "access_type",
			table: "roles",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "roles",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "referee_teams",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_teams",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "association_type",
			table: "referee_teams",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_teams",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_locations",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "referee_locations",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<int>(
			name: "association_type",
			table: "referee_locations",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_locations",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_certifications",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "referee_certifications",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_certifications",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "test_attempt_id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "question_id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "answer_id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "questions",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "seq_id",
			table: "questions",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<int>(
			name: "points_available",
			table: "questions",
			type: "INTEGER",
			nullable: false,
			defaultValueSql: "1",
			oldClrType: typeof(int),
			oldType: "integer",
			oldDefaultValueSql: "1");

		migrationBuilder.AlterColumn<string>(
			name: "feedback",
			table: "questions",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "questions",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "questions",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "policy_manager_user_terms",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "term_id",
			table: "policy_manager_user_terms",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_user_terms",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "policy_manager_terms",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_terms",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "policy_manager_portability_requests",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_portability_requests",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "youth_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "university_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "uncertified_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "total_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "total_referees_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "team_status_change_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "snitch_referees_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "scorekeeper_referees_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "bigint",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "inactive_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "head_referees_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "developing_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "competitive_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "community_teams_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "assistant_referees_count",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_body_stats",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "national_governing_body_admins",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "national_governing_body_admins",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_body_admins",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "region",
			table: "national_governing_bodies",
			type: "INTEGER",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "player_count",
			table: "national_governing_bodies",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<int>(
			name: "membership_status",
			table: "national_governing_bodies",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_bodies",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "languages",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "flipper_gates",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "flipper_features",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "user_id",
			table: "exported_csvs",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "exported_csvs",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "Xml",
			table: "DataProtectionKeys",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "FriendlyName",
			table: "DataProtectionKeys",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "Id",
			table: "DataProtectionKeys",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "version",
			table: "certifications",
			type: "INTEGER",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "level",
			table: "certifications",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "certifications",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "certification_payments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "certification_payments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "certification_payments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "question_id",
			table: "answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "answers",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<bool>(
			name: "correct",
			table: "answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "boolean");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "answers",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "metadata",
			table: "active_storage_blobs",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "byte_size",
			table: "active_storage_blobs",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "active_storage_blobs",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "record_id",
			table: "active_storage_attachments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "blob_id",
			table: "active_storage_attachments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "active_storage_attachments",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "bigint")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.CreateTable(
			name: "tournament_team_roster_entries",
			columns: table => new
			{
				id = table.Column<long>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				tournament_team_participant_id = table.Column<long>(type: "INTEGER", nullable: false),
				user_id = table.Column<long>(type: "INTEGER", nullable: false),
				role = table.Column<int>(type: "INTEGER", nullable: false),
				jersey_number = table.Column<string>(type: "character varying", maxLength: 5, nullable: true),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournament_team_roster_entries", x => x.id);
				table.CheckConstraint("chk_roster_entry_jersey_number", "role != 0 OR (role = 0 AND jersey_number IS NOT NULL)");
				table.ForeignKey(
					name: "fk_tournament_team_roster_entries_participant",
					column: x => x.tournament_team_participant_id,
					principalTable: "tournament_team_participants",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_tournament_team_roster_entries_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
			});

		migrationBuilder.CreateTable(
			name: "user_delicate_info",
			columns: table => new
			{
				id = table.Column<long>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				user_id = table.Column<long>(type: "INTEGER", nullable: false),
				gender = table.Column<string>(type: "character varying", nullable: true),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_user_delicate_info", x => x.id);
				table.ForeignKey(
					name: "fk_user_delicate_info_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "index_roster_entries_on_participant_and_user",
			table: "tournament_team_roster_entries",
			columns: new[] { "tournament_team_participant_id", "user_id" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_tournament_team_roster_entries_user_id",
			table: "tournament_team_roster_entries",
			column: "user_id");

		migrationBuilder.CreateIndex(
			name: "index_user_delicate_info_on_user_id",
			table: "user_delicate_info",
			column: "user_id",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "tournament_team_roster_entries");

		migrationBuilder.DropTable(
			name: "user_delicate_info");

		migrationBuilder.AlterColumn<int>(
			name: "sign_in_count",
			table: "users",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<bool>(
			name: "show_pronouns",
			table: "users",
			type: "boolean",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<string>(
			name: "last_sign_in_ip",
			table: "users",
			type: "inet",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "language_id",
			table: "users",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "invited_by_id",
			table: "users",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "invitations_count",
			table: "users",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "invitation_limit",
			table: "users",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "failed_attempts",
			table: "users",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<bool>(
			name: "export_name",
			table: "users",
			type: "boolean",
			nullable: true,
			defaultValueSql: "true",
			oldClrType: typeof(bool),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "true");

		migrationBuilder.AlterColumn<string>(
			name: "current_sign_in_ip",
			table: "users",
			type: "inet",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "bio",
			table: "users",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "admin",
			table: "users",
			type: "boolean",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "users",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "attribute_value",
			table: "user_attributes",
			type: "character varying(4096)",
			maxLength: 4096,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 4096);

		migrationBuilder.AlterColumn<string>(
			name: "key",
			table: "user_attributes",
			type: "character varying(128)",
			maxLength: 128,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 128);

		migrationBuilder.AlterColumn<string>(
			name: "prefix",
			table: "user_attributes",
			type: "character varying(16)",
			maxLength: 16,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 16);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "user_attributes",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "type",
			table: "tournaments",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<bool>(
			name: "is_private",
			table: "tournaments",
			type: "boolean",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournaments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_team_participants",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "tournament_team_participants",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_team_participants",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "added_by_user_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "tournament_manager_approval",
			table: "tournament_invites",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "tournament_id",
			table: "tournament_invites",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "participant_approval",
			table: "tournament_invites",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "initiator_user_id",
			table: "tournament_invites",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tournament_invites",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "time_limit",
			table: "tests",
			type: "integer",
			nullable: false,
			defaultValueSql: "18",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldDefaultValueSql: "18");

		migrationBuilder.AlterColumn<int>(
			name: "testable_question_count",
			table: "tests",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<bool>(
			name: "recertification",
			table: "tests",
			type: "boolean",
			nullable: true,
			defaultValueSql: "false",
			oldClrType: typeof(bool),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "false");

		migrationBuilder.AlterColumn<string>(
			name: "positive_feedback",
			table: "tests",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "new_language_id",
			table: "tests",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "negative_feedback",
			table: "tests",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "minimum_pass_percentage",
			table: "tests",
			type: "integer",
			nullable: false,
			defaultValueSql: "80",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldDefaultValueSql: "80");

		migrationBuilder.AlterColumn<int>(
			name: "level",
			table: "tests",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "tests",
			type: "text",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "tests",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "active",
			table: "tests",
			type: "boolean",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "tests",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<TimeOnly>(
			name: "time_started",
			table: "test_results",
			type: "time without time zone",
			nullable: true,
			oldClrType: typeof(TimeOnly),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<TimeOnly>(
			name: "time_finished",
			table: "test_results",
			type: "time without time zone",
			nullable: true,
			oldClrType: typeof(TimeOnly),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "test_level",
			table: "test_results",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "test_results",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "test_results",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "points_scored",
			table: "test_results",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "points_available",
			table: "test_results",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "percentage",
			table: "test_results",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<bool>(
			name: "passed",
			table: "test_results",
			type: "boolean",
			nullable: true,
			oldClrType: typeof(bool),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "minimum_pass_percentage",
			table: "test_results",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "test_results",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "test_level",
			table: "test_attempts",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "test_attempts",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "test_attempts",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "test_attempts",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "status",
			table: "teams",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "teams",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "group_affiliation",
			table: "teams",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "teams",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "team_status_changesets",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "team_status_changesets",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "team_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "team_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "team_managers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "ownable_id",
			table: "social_accounts",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "account_type",
			table: "social_accounts",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "social_accounts",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "roles",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "access_type",
			table: "roles",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "roles",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "team_id",
			table: "referee_teams",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_teams",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "association_type",
			table: "referee_teams",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_teams",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_locations",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "referee_locations",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "association_type",
			table: "referee_locations",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_locations",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_certifications",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "referee_certifications",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_certifications",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "test_attempt_id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "referee_id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "question_id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "answer_id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "referee_answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "test_id",
			table: "questions",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "seq_id",
			table: "questions",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "points_available",
			table: "questions",
			type: "integer",
			nullable: false,
			defaultValueSql: "1",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldDefaultValueSql: "1");

		migrationBuilder.AlterColumn<string>(
			name: "feedback",
			table: "questions",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "questions",
			type: "text",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "questions",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "policy_manager_user_terms",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "term_id",
			table: "policy_manager_user_terms",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_user_terms",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "policy_manager_terms",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_terms",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "policy_manager_portability_requests",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "policy_manager_portability_requests",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "youth_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "university_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "uncertified_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "total_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "total_referees_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "team_status_change_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "snitch_referees_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "scorekeeper_referees_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "national_governing_body_stats",
			type: "bigint",
			nullable: true,
			oldClrType: typeof(long),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "inactive_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "head_referees_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "developing_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "competitive_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "community_teams_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "assistant_referees_count",
			table: "national_governing_body_stats",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_body_stats",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "national_governing_body_admins",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "national_governing_body_id",
			table: "national_governing_body_admins",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_body_admins",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "region",
			table: "national_governing_bodies",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "player_count",
			table: "national_governing_bodies",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<int>(
			name: "membership_status",
			table: "national_governing_bodies",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "national_governing_bodies",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "languages",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "flipper_gates",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "flipper_features",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "user_id",
			table: "exported_csvs",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "exported_csvs",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "Xml",
			table: "DataProtectionKeys",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "FriendlyName",
			table: "DataProtectionKeys",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<int>(
			name: "Id",
			table: "DataProtectionKeys",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<int>(
			name: "version",
			table: "certifications",
			type: "integer",
			nullable: true,
			defaultValueSql: "0",
			oldClrType: typeof(int),
			oldType: "INTEGER",
			oldNullable: true,
			oldDefaultValueSql: "0");

		migrationBuilder.AlterColumn<int>(
			name: "level",
			table: "certifications",
			type: "integer",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "certifications",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "user_id",
			table: "certification_payments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "certification_id",
			table: "certification_payments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "certification_payments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "question_id",
			table: "answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "answers",
			type: "text",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<bool>(
			name: "correct",
			table: "answers",
			type: "boolean",
			nullable: false,
			oldClrType: typeof(bool),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "answers",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<string>(
			name: "metadata",
			table: "active_storage_blobs",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<long>(
			name: "byte_size",
			table: "active_storage_blobs",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "active_storage_blobs",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AlterColumn<long>(
			name: "record_id",
			table: "active_storage_attachments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "blob_id",
			table: "active_storage_attachments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER");

		migrationBuilder.AlterColumn<long>(
			name: "id",
			table: "active_storage_attachments",
			type: "bigint",
			nullable: false,
			oldClrType: typeof(long),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true)
			.OldAnnotation("Sqlite:Autoincrement", true);
	}
}
