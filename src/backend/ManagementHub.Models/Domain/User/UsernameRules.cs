using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Rules for user handles (usernames).
/// Usernames are lowercase, 3-30 characters long, consist of letters, digits, '.', '_' and '-',
/// and must start and end with a letter or a digit.
/// </summary>
public static partial class UsernameRules
{
	public const int MinLength = 3;
	public const int MaxLength = 30;

	/// <summary>
	/// Maximum length of an auto-generated base username, leaving room for a numeric suffix.
	/// </summary>
	public const int MaxBaseLength = 26;

	[GeneratedRegex("^[a-z0-9][a-z0-9._-]{1,28}[a-z0-9]$")]
	private static partial Regex UsernameRegex();

	[GeneratedRegex("[^a-z0-9]+")]
	private static partial Regex NonAlphanumericRegex();

	/// <summary>
	/// Normalizes user input into the canonical (lowercase, trimmed) username form.
	/// </summary>
	public static string Normalize(string username) => username.Trim().ToLowerInvariant();

	/// <summary>
	/// Checks whether a normalized username is valid.
	/// </summary>
	public static bool IsValid(string? username) => username != null && UsernameRegex().IsMatch(username);

	/// <summary>
	/// Whether the input should be treated as an email rather than a username.
	/// </summary>
	public static bool LooksLikeEmail(string? input) => input != null && input.Contains('@');

	/// <summary>
	/// Default username for a user that has no usable name.
	/// </summary>
	public static string FallbackFor(long userDbId) => $"user{userDbId}";

	/// <summary>
	/// Creates a base username (without a uniqueness suffix) from the user's names, e.g. "andres.vargas".
	/// Returns null if the names do not produce a usable username.
	/// </summary>
	public static string? CreateBase(string? firstName, string? lastName)
	{
		var first = Simplify(firstName);
		var last = Simplify(lastName);
		var combined = string.Join('.', new[] { first, last }.Where(p => p.Length > 0));
		if (combined.Length > MaxBaseLength)
		{
			combined = combined[..MaxBaseLength].TrimEnd('.');
		}

		return combined.Length >= MinLength ? combined : null;
	}

	/// <summary>
	/// Appends a numeric suffix to a base username. Suffix 1 returns the base unchanged.
	/// </summary>
	public static string WithSuffix(string baseUsername, int suffix) => suffix <= 1 ? baseUsername : $"{baseUsername}{suffix}";

	private static string Simplify(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}

		var replaced = name
			.Replace("ß", "ss").Replace("æ", "ae").Replace("Æ", "ae").Replace("œ", "oe").Replace("Œ", "oe")
			.Replace("ł", "l").Replace("Ł", "l").Replace("đ", "d").Replace("Đ", "d").Replace("ø", "o").Replace("Ø", "o");
		var decomposed = replaced.Normalize(NormalizationForm.FormD);
		var builder = new StringBuilder(decomposed.Length);
		foreach (var c in decomposed)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
			{
				builder.Append(c);
			}
		}

		var ascii = builder.ToString().ToLowerInvariant();
		return NonAlphanumericRegex().Replace(ascii, ".").Trim('.');
	}
}
