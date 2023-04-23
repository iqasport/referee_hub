using System;
using System.Collections.Generic;

namespace ManagementHub.Models.Misc;
public static class EnumerableExtensions
{
	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable) => Shuffle(enumerable, (int)DateTime.Now.Ticks);

	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <param name="seed">Seed for the RNG.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable, int seed)
	{
		// source: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle ("inside out" section)
		var rng = new Random(seed);
		// we make an assumption here that the collections we will be shuffling on avarage will be under 64 elements.
		var output = new List<T>(64);

		foreach (var item in enumerable)
		{
			var j = rng.Next(0, output.Count + 1); // 0 <= j <= count
			if (j == output.Count)
			{
				output.Add(item);
			}
			else
			{
				output.Add(output[j]);
				output[j] = item;
			}
		}

		return output;
	}
}
