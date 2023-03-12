using System;

namespace ManagementHub.Models.Domain.Language;

public record struct LanguageIdentifier(string Lang, string? Region = null)
{
	/// <summary>
	/// Default language: en-US
	/// </summary>
	public static LanguageIdentifier Default => new LanguageIdentifier("en", "US");
}
