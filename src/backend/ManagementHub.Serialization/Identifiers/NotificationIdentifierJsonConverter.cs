using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Notification;

namespace ManagementHub.Serialization.Identifiers;

public sealed class NotificationIdentifierJsonConverter : JsonConverter<NotificationIdentifier>
{
	public override bool HandleNull => false;

	public override NotificationIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !NotificationIdentifier.TryParse(currentElement, out var notificationId))
		{
			throw new JsonException($"Could not read a {nameof(NotificationIdentifier)}.");
		}

		return notificationId;
	}

	public override void Write(Utf8JsonWriter writer, NotificationIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}