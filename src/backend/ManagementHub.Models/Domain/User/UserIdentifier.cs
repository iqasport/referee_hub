using System;
using System.Runtime.InteropServices;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Identifier of a user.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct UserIdentifier(Guid UniqueId) : IIdentifiable
{
	private const string IdPrefix = "U_";
	private const int GuidAsStringLength = 26;

	public long Id => this.ToLegacyUserId();

	public bool IsLegacy
	{
		get
		{
			var uniqueId = this.UniqueId;
			return GuidAsLongSpan(ref uniqueId)[1] == 0;
		}
	}

	/// <summary>
	/// Returns the serialized user identifier.
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
	public static UserIdentifier FromLegacyUserId(long id)
	{
		Guid uniqueId = new Guid();
		GuidAsLongSpan(ref uniqueId)[0] = id;
		var userId = new UserIdentifier(uniqueId);
		return userId;
	}

	/// <summary>
	/// Creates a new unique user identifier.
	/// </summary>
	public static UserIdentifier NewUserId() => new UserIdentifier(Guid.NewGuid());

	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out UserIdentifier result)
	{
		result = default;

		if (value.Length != GuidAsStringLength + IdPrefix.Length)
			return false;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		var uniqueId = value.AsSpan().Slice(IdPrefix.Length).GuidFromBase32String();
		if (uniqueId == default)
			return false;

		result = new UserIdentifier(uniqueId);
		return true;
	}

	public static UserIdentifier Parse(string value) => TryParse(value, out UserIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(UserIdentifier)}");

	private static Span<long> GuidAsLongSpan(ref Guid guid) =>
		MemoryMarshal.Cast<Guid, long>(MemoryMarshal.CreateSpan(ref guid, 1));
}
