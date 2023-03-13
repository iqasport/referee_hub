using System;

namespace ManagementHub.Models.Domain.Language;

public record struct LanguageIdentifier(string Lang, string? Region = null)
{
	/// <summary>
	/// Default language: en-US
	/// </summary>
	public static LanguageIdentifier Default => new LanguageIdentifier("en", "US");

	public static bool TryParse(string value, out LanguageIdentifier langId)
	{
		langId = default;
		if (string.IsNullOrWhiteSpace(value)) return false;
		
		var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var lang = parts[0];
		var region = parts.Length > 1 ? parts[1] : null;
		
		langId = new LanguageIdentifier(lang, region);
		return true;
	}

	public override string ToString() => $"{this.Lang}{(this.Region != null ? "-" : string.Empty)}{this.Region ?? string.Empty}";
}
