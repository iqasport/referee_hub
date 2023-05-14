using System;

namespace ManagementHub.Models.Domain.Ngb;

/// <summary>
/// Identifier of a National Governing Body.
/// </summary>
public record struct NgbIdentifier(string NgbCode)
{
	public override string ToString() => this.NgbCode;

	public static bool TryParse(string value, out NgbIdentifier identifier)
	{
		if (value == null || value.Length != 3)
		{
			identifier = default;
			return false;
		}

		identifier = new NgbIdentifier(value.ToUpper());
		return true;
	}

	public static NgbIdentifier Parse(string value) => TryParse(value, out var id) ? id : throw new FormatException($"The string is not a valid {nameof(NgbIdentifier)}");
}
