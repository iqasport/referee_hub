using System;
using System.Text.RegularExpressions;

namespace ManagementHub.Models.Domain.General;

/// <summary>
/// Email class wraps the email string providing validation and common representation.
/// </summary>
public partial struct Email
{
	// see https://stackoverflow.com/a/201378
	private const string EmailValidationRegex = "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])";

	[GeneratedRegex(EmailValidationRegex)]
	private static partial Regex EmailValidation();

	public Email(string email)
	{
		email = email.Trim().ToLower(); // normalize email string

		if (!EmailValidation().IsMatch(email))
		{
			throw new ArgumentException("Provided value is not a valid email address", nameof(email));
		}

		this.Value = email;
	}

	public string Value { get; }

	public override string ToString() => this.Value;

	public override bool Equals(object? obj)
	{
		return obj is Email other && this == other;
	}

	public override int GetHashCode() => this.Value.GetHashCode();

	public static bool TryParse(string value, out Email email)
	{
		try
		{
			email = new Email(value);
			return true;
		}
		catch (ArgumentException)
		{
			email = default;
			return false;
		}
	}

	/// <summary>
	/// Two emails are equal if the underlying string value is equal (ignoring case).
	/// </summary>
	public static bool operator ==(Email a, Email b) => string.Equals(a.Value, b.Value, StringComparison.OrdinalIgnoreCase);
	public static bool operator !=(Email a, Email b) => !(a == b);
}
