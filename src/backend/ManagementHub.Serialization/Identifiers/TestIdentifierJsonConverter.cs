using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="TestIdentifier"/> as a string.
/// </summary>
public sealed class TestIdentifierJsonConverter : JsonConverter<TestIdentifier>
{
	public override bool HandleNull => false;
	public override TestIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !TestIdentifier.TryParse(currentElement, out TestIdentifier userId))
		{
			throw new JsonException($"Could not read a {nameof(TestIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, TestIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
