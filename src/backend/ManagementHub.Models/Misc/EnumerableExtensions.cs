using System;
using System.Collections.Generic;
using System.Threading;

namespace ManagementHub.Models.Misc;
public static class EnumerableExtensions
{
	private static readonly AsyncLocal<Random?> asynRandom = new AsyncLocal<Random?>();
	public static void SetAsyncRandomContext() => asynRandom.Value = new Random();
	public static void SetAsyncRandomContext(int seed) => asynRandom.Value = new Random(seed);

	/// <summary>
	/// Shuffles the enumerable collection using Fisher–Yates algorithm.
	/// </summary>
	/// <param name="enumerable">Enumerble collection of finite size.</param>
	/// <returns>A list containing items from the collection in random order.</returns>
	public static List<T> Shuffle<T>(this IEnumerable<T> enumerable)
	{
		if (asynRandom.Value is null)
			return enumerable.Shuffle((int)DateTime.Now.Ticks);
		else
			return enumerable.Shuffle(asynRandom.Value);
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
