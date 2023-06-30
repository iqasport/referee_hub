using System;

namespace ManagementHub.Models.Domain.Tests;

/// <summary>
/// Struct encapsulating a percentage value.
/// </summary>
public struct Percentage
{
	public Percentage(int value)
	{
		if (value < 0 || value > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		this.Value = value;
	}

	public int Value { get; }

	public override string ToString() => $"{this.Value}%";

	public static implicit operator Percentage(int value) => new(value);
	public static explicit operator int(Percentage value) => value.Value;

	public static bool operator >=(Percentage value1, Percentage value2) => value1.Value >= value2.Value;
	public static bool operator <=(Percentage value1, Percentage value2) => value1.Value <= value2.Value;
}
