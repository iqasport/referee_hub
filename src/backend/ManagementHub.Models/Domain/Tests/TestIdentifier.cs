﻿using System;
using System.Runtime.InteropServices;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Domain.Tests;

public record struct TestIdentifier(Guid UniqueId)
{
	private const string IdPrefix = "T_";
	private const int GuidAsStringLength = 26;
	public long Id => this.ToLegacyUserId();

	/// <summary>
	/// Returns the serialized test identifier.
	/// </summary>
	public override string ToString()
	{
		return $"{IdPrefix}{this.UniqueId.ToBase32String()}";
	}

	/// <summary>
	/// Uses upper 8 bytes of the <see cref="UniqueId"/> as the id.
	/// </summary>
	public long ToLegacyUserId()
	{
		var uniqueId = this.UniqueId;
		return GuidAsLongSpan(ref uniqueId)[0];
	}

	/// <summary>
	/// Uses upper 8 bytes of the <see cref="UniqueId"/> as the id.
	/// </summary>
	public static TestIdentifier FromLegacyTestId(long id)
	{
		Guid uniqueId = new Guid();
		GuidAsLongSpan(ref uniqueId)[0] = id;
		var userId = new TestIdentifier(uniqueId);
		return userId;
	}

	/// <summary>
	/// Creates a new unique test identifier.
	/// </summary>
	public static TestIdentifier NewTestId() => new TestIdentifier(Guid.NewGuid());

	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out TestIdentifier result)
	{
		result = default;

		if (value.Length != GuidAsStringLength + IdPrefix.Length)
			return false;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		var uniqueId = value.AsSpan().Slice(IdPrefix.Length).GuidFromBase32String();
		if (uniqueId == default)
			return false;

		result = new TestIdentifier(uniqueId);
		return true;
	}

	public static TestIdentifier Parse(string value) => TryParse(value, out TestIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(TestIdentifier)}");

	private static Span<long> GuidAsLongSpan(ref Guid guid) =>
		MemoryMarshal.Cast<Guid, long>(MemoryMarshal.CreateSpan(ref guid, 1));
}
