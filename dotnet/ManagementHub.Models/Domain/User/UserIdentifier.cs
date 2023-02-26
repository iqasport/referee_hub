using System;
using System.Runtime.InteropServices;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Identifier of a user.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct UserIdentifier(Guid UniqueId) : IIdentifiable
{
	private const string IdPrefix = "u-";
	private const int GuidAsStringLength = 36;
	public long Id => this.ToLegacyUserId();

	public override string ToString()
	{
		return $"{IdPrefix}{UniqueId}";
	}

	/// <summary>
	/// Uses upper 8 bytes of the <see cref="UniqueId"/> as the id.
	/// </summary>
	public long ToLegacyUserId()
	{
		var uniqueId = UniqueId;
		return GuidAsLongSpan(ref uniqueId)[0];
	}

	/// <summary>
	/// Uses upper 8 bytes of the <see cref="UniqueId"/> as the id.
	/// </summary>
	public static UserIdentifier FromLegacyUserId(long id)
	{
		Guid uniqueId = new Guid();
		GuidAsLongSpan(ref uniqueId)[0] = id;
		var userId = new UserIdentifier();
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

		if(!Guid.TryParse(value.AsSpan().Slice(IdPrefix.Length), out var uniqueId))
			return false;

		result = new UserIdentifier(uniqueId);
		return true;
	}

	private static Span<long> GuidAsLongSpan(ref Guid guid) =>
		MemoryMarshal.Cast<Guid, long>(MemoryMarshal.CreateSpan(ref guid, 1));
}
