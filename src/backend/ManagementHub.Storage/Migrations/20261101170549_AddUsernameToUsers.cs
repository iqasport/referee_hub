using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// Adds a unique, lowercase <c>username</c> column to users and backfills existing users
/// with a default of <c>firstname.lastname</c> (accents transliterated), appending a numeric
/// suffix on collisions. Users without a usable name receive <c>user{id}</c>.
/// </remarks>
public partial class AddUsernameToUsers : Migration
{
	private static readonly (char Target, string Sources)[] TransliterationGroups =
	[
		('a', "áàâäãåāăą"),
		('c', "çćč"),
		('d', "ďđ"),
		('e', "éèêëēėęě"),
		('i', "íìîïīį"),
		('l', "ł"),
		('n', "ñńň"),
		('o', "óòôöõøōő"),
		('r', "ŕř"),
		('s', "śšş"),
		('t', "ťţ"),
		('u', "úùûüūůűų"),
		('y', "ýÿ"),
		('z', "žźż"),
	];

	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "username",
			table: "users",
			type: "varchar(30)",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "index_users_on_username",
			table: "users",
			column: "username",
			unique: true);

		var (from, to) = BuildTranslateArguments();

		migrationBuilder.Sql($$"""
			DO $$
			DECLARE
				r RECORD;
				first_part TEXT;
				last_part TEXT;
				base TEXT;
				candidate TEXT;
				suffix INT;
			BEGIN
				FOR r IN SELECT id, first_name, last_name FROM users WHERE username IS NULL ORDER BY id LOOP
					first_part := lower(translate(replace(replace(replace(coalesce(r.first_name, ''), 'ß', 'ss'), 'æ', 'ae'), 'œ', 'oe'), '{{from}}', '{{to}}'));
					first_part := btrim(regexp_replace(first_part, '[^a-z0-9]+', '.', 'g'), '.');
					last_part := lower(translate(replace(replace(replace(coalesce(r.last_name, ''), 'ß', 'ss'), 'æ', 'ae'), 'œ', 'oe'), '{{from}}', '{{to}}'));
					last_part := btrim(regexp_replace(last_part, '[^a-z0-9]+', '.', 'g'), '.');

					base := concat_ws('.', NULLIF(first_part, ''), NULLIF(last_part, ''));
					base := rtrim(left(base, 26), '.');

					IF base IS NULL OR length(base) < 3 THEN
						base := 'user' || r.id;
					END IF;

					candidate := base;
					suffix := 1;
					WHILE EXISTS (SELECT 1 FROM users WHERE username = candidate) LOOP
						suffix := suffix + 1;
						candidate := base || suffix;
					END LOOP;

					UPDATE users SET username = candidate WHERE id = r.id;
				END LOOP;
			END $$;
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "index_users_on_username",
			table: "users");

		migrationBuilder.DropColumn(
			name: "username",
			table: "users");
	}

	private static (string From, string To) BuildTranslateArguments()
	{
		var from = new StringBuilder();
		var to = new StringBuilder();
		foreach (var (target, sources) in TransliterationGroups)
		{
			foreach (var source in sources)
			{
				from.Append(source);
				to.Append(target);
				var upper = char.ToUpperInvariant(source);
				if (upper != source && !sources.Contains(upper))
				{
					from.Append(upper);
					to.Append(target);
				}
			}
		}

		return (from.ToString(), to.ToString());
	}
}
