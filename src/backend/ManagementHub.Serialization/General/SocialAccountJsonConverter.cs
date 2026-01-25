using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Enums;

namespace ManagementHub.Serialization.General;

/// <summary>
/// Custom JSON converter for SocialAccount that handles URL normalization.
/// Accepts URLs with or without https:// prefix and normalizes them.
/// </summary>
public class SocialAccountJsonConverter : JsonConverter<SocialAccount>
{
	public override SocialAccount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException("Expected start of object");
		}

		string? url = null;
		SocialAccountType? type = null;

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				break;
			}

			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				var propertyName = reader.GetString();
				reader.Read();

				if (propertyName?.Equals("url", StringComparison.OrdinalIgnoreCase) == true ||
					propertyName?.Equals("Url", StringComparison.OrdinalIgnoreCase) == true)
				{
					url = reader.GetString();
				}
				else if (propertyName?.Equals("type", StringComparison.OrdinalIgnoreCase) == true ||
						propertyName?.Equals("Type", StringComparison.OrdinalIgnoreCase) == true)
				{
					var typeString = reader.GetString();
					if (typeString != null && Enum.TryParse<SocialAccountType>(typeString, true, out var parsedType))
					{
						type = parsedType;
					}
				}
			}
		}

		if (string.IsNullOrWhiteSpace(url))
		{
			throw new JsonException("Social account URL is required");
		}

		if (!type.HasValue)
		{
			throw new JsonException("Social account type is required");
		}

		// Try to create URI - if it fails, try adding https://
		Uri? uri = null;
		if (Uri.TryCreate(url, UriKind.Absolute, out uri))
		{
			// Valid absolute URL
			if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
			{
				throw new JsonException($"Social account URL must use http or https protocol: {url}");
			}
		}
		else if (Uri.TryCreate($"https://{url}", UriKind.Absolute, out uri) && uri.Host.Contains('.'))
		{
			// Valid after adding https://
		}
		else
		{
			throw new JsonException($"Invalid social account URL: {url}");
		}

		return new SocialAccount(uri, type.Value);
	}

	public override void Write(Utf8JsonWriter writer, SocialAccount value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString("url", value.Url.ToString());
		writer.WriteString("type", value.Type.ToString().ToLowerInvariant());
		writer.WriteEndObject();
	}
}
