// COPIED FROM https://stackoverflow.com/a/7135008
// slightly modified

using System;

namespace ManagementHub.Storage.BlobStorage;

/// <summary>
/// Base32 encoding uses A-Z and 2-7 ASCII characters to repsrent 5-bit encoding.
/// Commonly used for file names or where case insensitive formatting is required.
/// </summary>
public class Base32Encoding
{
	public static byte[] ToBytes(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return Array.Empty<byte>();
		}

		input = input.TrimEnd('='); //remove padding characters
		int byteCount = input.Length * 5 / 8; //this must be TRUNCATED
		byte[] returnArray = new byte[byteCount];

		byte curByte = 0, bitsRemaining = 8;
		int arrayIndex = 0;

		foreach (char c in input)
		{
			int cValue = CharToValue(c);
			int mask;
			if (bitsRemaining > 5)
			{
				mask = cValue << (bitsRemaining - 5);
				curByte = (byte)(curByte | mask);
				bitsRemaining -= 5;
			}
			else
			{
				mask = cValue >> (5 - bitsRemaining);
				curByte = (byte)(curByte | mask);
				returnArray[arrayIndex++] = curByte;
				curByte = (byte)(cValue << (3 + bitsRemaining));
				bitsRemaining += 3;
			}
		}

		//if we didn't end with a full byte
		if (arrayIndex != byteCount)
		{
			returnArray[arrayIndex] = curByte;
		}

		return returnArray;
	}

	public static string ToString(byte[] input)
	{
		if (input.Length == 0)
		{
			return string.Empty;
		}

		int charCount = (int)Math.Ceiling(input.Length / 5d) * 8;
		char[] returnArray = new char[charCount];

		byte nextChar = 0, bitsRemaining = 5;
		int arrayIndex = 0;

		foreach (byte b in input)
		{
			nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
			returnArray[arrayIndex++] = ValueToChar(nextChar);

			if (bitsRemaining < 4)
			{
				nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
				returnArray[arrayIndex++] = ValueToChar(nextChar);
				bitsRemaining += 5;
			}

			bitsRemaining -= 3;
			nextChar = (byte)((b << bitsRemaining) & 31);
		}

		//if we didn't end with a full char
		if (arrayIndex != charCount)
		{
			returnArray[arrayIndex++] = ValueToChar(nextChar);
			while (arrayIndex != charCount) returnArray[arrayIndex++] = '='; //padding
		}

		return new string(returnArray);
	}

	private static int CharToValue(char value)
	{
		//65-90 == uppercase letters
		if (value < 91 && value > 64)
		{
			return value - 65;
		}
		//50-55 == numbers 2-7
		if (value < 56 && value > 49)
		{
			return value - 24;
		}
		//97-122 == lowercase letters
		if (value < 123 && value > 96)
		{
			return value - 97;
		}

		throw new ArgumentException("Character is not a Base32 character.", nameof(value));
	}

	private static char ValueToChar(byte value)
	{
		if (value < 26)
		{
			return (char)(value + 97);
		}

		if (value < 32)
		{
			return (char)(value + 24);
		}

		throw new ArgumentException("Byte is not a value Base32 value.", nameof(value));
	}

}

