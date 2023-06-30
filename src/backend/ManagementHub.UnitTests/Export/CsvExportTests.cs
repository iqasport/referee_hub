using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Processing.Export;
using Xunit;
using Xunit.Abstractions;

public class CsvExportTests
{
	private readonly ITestOutputHelper testOutput;

	public CsvExportTests(ITestOutputHelper testOutput)
	{
		this.testOutput = testOutput;
	}

	[Fact]
	public void CanStreamCsv()
	{
		const int dataSize = 100;
		var collection = this.GetData(dataSize);
		var words = new[] { "apple", "banana", "cat", "dog", "even", "fold" };

		var stream = collection.ExportAsyncEnumerableAsCsv((i) => new CsvRow
		{
			Number = i,
			Word = words[i % words.Length],
		});

		int linesRead = 0;
		using var reader = new StreamReader(stream);
		while (!reader.EndOfStream)
		{
			this.testOutput.WriteLine(reader.ReadLine());
			linesRead++;
		}

		Assert.Equal(dataSize + 1 /* header row */, linesRead);
	}

	private async IAsyncEnumerable<int> GetData(int count)
	{
		foreach (var x in Enumerable.Range(0, count))
		{
			await Task.Yield();
			yield return x;
		}
	}

	private class CsvRow
	{
		public int Number { get; set; }
		public string Word { get; set; } = string.Empty;
	}
}
