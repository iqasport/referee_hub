using System;

namespace ManagementHub.Models.Domain.Tests;

public record struct TestAttemptIdentifier(Ulid UniqueId)
{
	private const string IdPrefix = "TAT_";
	private const int UlidAsStringLength = 26;

	/// <summary>
	/// Returns the serialized test attempt identifier.
	/// </summary>
	public override string ToString()
	{
		return $"{IdPrefix}{this.UniqueId}";
	}

	/// <summary>
	/// Creates a new unique test attempt identifier.
	/// </summary>
	public static TestAttemptIdentifier NewTestAttemptId() => new TestAttemptIdentifier(Ulid.NewUlid());

	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out TestAttemptIdentifier result)
	{
		result = default;

		if (value.Length != UlidAsStringLength + IdPrefix.Length)
			return false;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!Ulid.TryParse(value.AsSpan().Slice(IdPrefix.Length), out var uniqueId))
			return false;

		result = new TestAttemptIdentifier(uniqueId);
		return true;
	}

	public static TestAttemptIdentifier Parse(string value) => TryParse(value, out TestAttemptIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(TestAttemptIdentifier)}");
}
