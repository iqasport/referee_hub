using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="TestAttemptIdentifier"/> as a string.
/// </summary>
public sealed class TestAttemptIdentifierJsonConverter : JsonConverter<TestAttemptIdentifier>
{
	public override bool HandleNull => false;
	public override TestAttemptIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !TestAttemptIdentifier.TryParse(currentElement, out TestAttemptIdentifier userId))
		{
			throw new JsonException($"Could not read a {nameof(TestAttemptIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, TestAttemptIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
