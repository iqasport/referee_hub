using System;
using System.Runtime.InteropServices;

namespace ManagementHub.Models.Misc;
public static class GuidExtensions
{
	public static string ToBase32String(this Guid id)
	{
		return Base32Encoding.ToString(MemoryMarshal.AsBytes(new Span<Guid>(ref id))).Trim('=');
	}

	public static Guid GuidFromBase32String(this string input) => GuidFromBase32String((ReadOnlySpan<char>)input);

	public static Guid GuidFromBase32String(this ReadOnlySpan<char> input)
	{
		Guid id = default;
		Base32Encoding.ToBytes(input, MemoryMarshal.AsBytes(new Span<Guid>(ref id)));
		return id;
	}
}
