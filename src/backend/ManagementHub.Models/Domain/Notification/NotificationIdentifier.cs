using System;

namespace ManagementHub.Models.Domain.Notification;

public record struct NotificationIdentifier(Ulid UniqueId)
{
	private const string IdPrefix = "N_";
	private const int UlidAsStringLength = 26;

	/// <summary>
	/// Returns the serialized notification identifier.
	/// </summary>
	public override string ToString()
	{
		return $"{IdPrefix}{this.UniqueId}";
	}

	/// <summary>
	/// Creates a new unique notification identifier.
	/// </summary>
	public static NotificationIdentifier NewNotificationId() => new NotificationIdentifier(Ulid.NewUlid());

	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out NotificationIdentifier result)
	{
		result = default;

		if (value.Length != UlidAsStringLength + IdPrefix.Length)
			return false;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!Ulid.TryParse(value.AsSpan().Slice(IdPrefix.Length), out var uniqueId))
			return false;

		result = new NotificationIdentifier(uniqueId);
		return true;
	}

	public static NotificationIdentifier Parse(string value) => TryParse(value, out NotificationIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(NotificationIdentifier)}");
}
