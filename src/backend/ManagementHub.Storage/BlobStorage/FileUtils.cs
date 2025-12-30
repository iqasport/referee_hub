using System.Security.Cryptography;
using ManagementHub.Models.Misc;

namespace ManagementHub.Storage.BlobStorage;

public static class FileUtils
{
	/// <summary>
	/// Generates a random file name (no extension) of 32 characters.
	/// </summary>
	public static string GenerateRandomFileName()
	{
		// Gets 20 random bytes
		// the number 20 is chosen in order to get a 32 character string in base32 which uses 5 bits per character.
		// 32 * 5 / 8 = 20
		const int byteCount = 20;
		var randomBytes = RandomNumberGenerator.GetBytes(byteCount);
		return Base32Encoding.ToString(randomBytes);
	}
}
