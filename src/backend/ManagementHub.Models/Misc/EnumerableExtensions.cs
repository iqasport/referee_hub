using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ManagementHub.Models.Misc;
public static class EnumerableExtensions
{
	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.Shuffle(Random.Shared);
	}

	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <param name="seed">Seed for the RNG.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable, int seed) => enumerable.Shuffle(new Random(seed));

	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <param name="rng">Random number generator.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	[SuppressMessage("Security", "SCS0005:Weak random number generator.", Justification = "RNG used for shuffling a collection and not for crypto.")]
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable, Random rng)
	{
		// source: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle ("inside out" section)
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
