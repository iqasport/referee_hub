using System.Text;

namespace ManagementHub.Models.Misc;

public static class StringExtensions
{
	public static string AsDisplayName(this string input)
	{
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < input.Length; i++)
		{
			char currentChar = input[i];

			if (i > 0 && char.IsUpper(currentChar))
			{
				sb.Append(' ');
			}

			sb.Append(currentChar);
		}

		return sb.ToString();
	}
}
