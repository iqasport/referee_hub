using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.User.Roles;

namespace ManagementHub.Serialization.Roles;

/// <summary>
/// Serializes a <see cref="IUserRole"/> as a custom object.
/// </summary>
public sealed class UserRoleJsonConverter : JsonConverter<IUserRole>
{
	public const string RolePropertyName = "roleType";

	private static readonly Dictionary<string, Type> rolesMapping = new(StringComparer.OrdinalIgnoreCase)
	{
		["IqaAdmin"] = typeof(IqaAdminRole),
		["NgbAdmin"] = typeof(NgbAdminRole),
		["NgbStatsManager"] = typeof(NgbStatsManagerRole),
		["NgbStatsViewer"] = typeof(NgbStatsViewerRole),
		["NgbUserAdmin"] = typeof(NgbUserAdminRole),
		["RefereeAdmin"] = typeof(RefereeAdminRole),
		["RefereeManager"] = typeof(RefereeManagerRole),
		["Referee"] = typeof(RefereeRole),
		["RefereeViewer"] = typeof(RefereeViewerRole),
		["TechAdmin"] = typeof(TechAdminRole),
		["TestAdmin"] = typeof(TestAdminRole),
		["TournamentManager"] = typeof(TournamentManagerRole),
	};

	public override bool HandleNull => true;

	public override IUserRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var headerReader = reader; // copy reference to beginning of the stream

		var roleType = string.Empty;

		headerReader.Read();
		if (headerReader.TokenType != JsonTokenType.StartObject)
			throw new JsonException();

		while (headerReader.Read())
		{
			if (headerReader.TokenType == JsonTokenType.PropertyName &&
				headerReader.ValueTextEquals(RolePropertyName))
			{
				headerReader.Read();
				roleType = headerReader.GetString();
				break;
			}
		}

		if (string.IsNullOrEmpty(roleType))
			throw new JsonException();

		if (!rolesMapping.TryGetValue(roleType, out Type? type))
			throw new NotSupportedException("Unexpected role type.");

		object? roleInstance = JsonSerializer.Deserialize(ref reader, type, options);
		return (IUserRole)roleInstance!;
	}

	public override void Write(Utf8JsonWriter writer, IUserRole value, JsonSerializerOptions options)
	{
		var type = value.GetType();
		var roleType = rolesMapping.FirstOrDefault(kvp => kvp.Value == type).Key;

		if (string.IsNullOrEmpty(roleType))
			throw new NotSupportedException("Unexpected role object of unknown type.");

		using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
		writer.WriteStartObject();
		writer.WriteString(RolePropertyName, roleType);

		foreach (var element in jsonDocument.RootElement.EnumerateObject())
		{
			element.WriteTo(writer);
		}

		writer.WriteEndObject();
	}
}
