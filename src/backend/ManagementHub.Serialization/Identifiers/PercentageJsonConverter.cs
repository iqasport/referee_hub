using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="Percentage"/> as a number.
/// </summary>
public sealed class PercentageJsonConverter : JsonConverter<Percentage>
{
	public override bool HandleNull => false;
	public override Percentage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetInt32();
	}

	public override void Write(Utf8JsonWriter writer, Percentage value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue((int)value);
	}
}
