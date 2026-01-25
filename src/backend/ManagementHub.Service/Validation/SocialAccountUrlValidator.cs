using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ManagementHub.Models.Domain.General;

namespace ManagementHub.Service.Validation;

/// <summary>
/// Validation attribute for social account URLs.
/// Validates that URLs are valid and automatically adds https:// if missing.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ValidateSocialAccountUrlsAttribute : ValidationAttribute
{
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value == null)
		{
			return ValidationResult.Success;
		}

		if (value is not IEnumerable<SocialAccount> socialAccounts)
		{
			return new ValidationResult("Property must be an enumerable of SocialAccount");
		}

		var errors = new List<string>();
		foreach (var account in socialAccounts)
		{
			var validationResult = SocialAccountUrlValidator.ValidateUrl(account.Url.OriginalString);
			if (!validationResult.IsValid)
			{
				errors.Add(validationResult.ErrorMessage!);
			}
		}

		if (errors.Count > 0)
		{
			return new ValidationResult(string.Join("; ", errors));
		}

		return ValidationResult.Success;
	}
}

/// <summary>
/// Helper class for validating and normalizing social account URLs.
/// </summary>
public static class SocialAccountUrlValidator
{
	/// <summary>
	/// Validates a URL string and returns whether it's valid.
	/// </summary>
	public static (bool IsValid, string? ErrorMessage) ValidateUrl(string url)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return (false, "URL cannot be empty");
		}

		// Try to parse as-is
		if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
		{
			// Valid URL - ensure it has http or https scheme
			if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
			{
				return (false, $"URL must use http or https protocol: {url}");
			}
			return (true, null);
		}

		// Try adding https://
		if (Uri.TryCreate($"https://{url}", UriKind.Absolute, out uri))
		{
			// Valid after adding https:// - check if host looks reasonable
			if (uri.Scheme == Uri.UriSchemeHttps && IsValidHost(uri.Host))
			{
				return (true, null);
			}
		}

		return (false, $"Invalid URL: {url}");
	}

	/// <summary>
	/// Normalizes a URL by adding https:// if missing.
	/// Returns null if the URL is invalid.
	/// </summary>
	public static string? NormalizeUrl(string url)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return null;
		}

		// If already a valid absolute URL, return as-is
		if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
			(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
		{
			return uri.ToString();
		}

		// Try adding https://
		if (Uri.TryCreate($"https://{url}", UriKind.Absolute, out uri) &&
			uri.Scheme == Uri.UriSchemeHttps && IsValidHost(uri.Host))
		{
			return uri.ToString();
		}

		return null;
	}

	/// <summary>
	/// Validates that a host looks reasonable for a URL.
	/// Checks for basic domain structure (contains at least one dot).
	/// </summary>
	private static bool IsValidHost(string host)
	{
		if (string.IsNullOrWhiteSpace(host))
		{
			return false;
		}

		// Basic check: host should contain at least one dot (e.g., example.com)
		// This filters out obviously invalid hosts like "localhost" or single words
		return host.Contains('.') && host.Length > 3;
	}

	/// <summary>
	/// Validates and normalizes a collection of social accounts.
	/// Returns normalized accounts and any validation errors.
	/// </summary>
	public static (IEnumerable<SocialAccount> NormalizedAccounts, List<string> Errors) ValidateAndNormalize(
		IEnumerable<SocialAccount> socialAccounts)
	{
		var normalized = new List<SocialAccount>();
		var errors = new List<string>();

		foreach (var account in socialAccounts)
		{
			var normalizedUrl = NormalizeUrl(account.Url.OriginalString);
			if (normalizedUrl == null)
			{
				errors.Add($"Invalid URL: {account.Url.OriginalString}");
				continue;
			}

			normalized.Add(new SocialAccount(new Uri(normalizedUrl), account.Type));
		}

		return (normalized, errors);
	}
}
