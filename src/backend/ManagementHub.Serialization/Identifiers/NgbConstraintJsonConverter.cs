using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="NgbConstraint"/> as a string or array of strings.
/// </summary>
public sealed class NgbConstraintJsonConverter : JsonConverter<NgbConstraint>
{
	public override bool HandleNull => false;
	public override NgbConstraint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, NgbConstraint value, JsonSerializerOptions options)
	{
		if (value.AppliesToAny)
		{
			writer.WriteStringValue("ANY");
		}
		else if (value is IEnumerable<NgbIdentifier> ngbIds)
		{
			var ngbIdsList = ngbIds.ToList();
			if (ngbIdsList.Count == 1)
			{
				writer.WriteStringValue(ngbIdsList.First().ToString());
			}
			else
			{
				writer.WriteStartArray();
				foreach(var ngbId in ngbIdsList)
				{
					writer.WriteStringValue(ngbId.ToString());
				}
				writer.WriteEndArray();
			}
		}
	}
}
