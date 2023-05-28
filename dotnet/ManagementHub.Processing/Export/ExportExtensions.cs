using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public static class ExportExtensions
{
	/// <summary>
	/// Processes an async enumerable <paramref name="source"/>, applying the <paramref name="selector"/> over every element
	/// and writing the resulting value as rows of a CSV file.
	/// The CSV content is written into a pipe stream that can be read by another thread.
	/// </summary>
	/// <param name="source">Source collection.</param>
	/// <param name="selector">Selector applied over each element to convert data to output schema.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <param name="logger">Optional logger to log exceptions.</param>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TOutput"></typeparam>
	/// <returns>A stream with the CSV contents.</returns>
	public static Stream ExportAsyncEnumerableAsCsv<TSource, TOutput>(this IAsyncEnumerable<TSource> source, Func<TSource, TOutput> selector, CancellationToken cancellationToken = default, ILogger? logger = null)
	{
		var writeStream = new AnonymousPipeServerStream(PipeDirection.Out);
		var readStream = new AnonymousPipeClientStream(PipeDirection.In, writeStream.GetClientHandleAsString());
		var csvStream = new CsvHelper.CsvWriter(new StreamWriter(writeStream), CultureInfo.InvariantCulture);

		_ = Task.Run(async () =>
		{
			try
			{
				csvStream.WriteHeader<TOutput>();
				await foreach (var item in source)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await csvStream.NextRecordAsync();
					csvStream.WriteRecord<TOutput>(selector(item));
				}

				await csvStream.FlushAsync();
			}
			catch (OperationCanceledException cex)
			{
				logger?.LogWarning(0, cex, "Cancellation was requested while exporting data to CSV.");
			}
			catch (Exception ex)
			{
				logger?.LogError(0, ex, "Error occurred while exporting data to CSV.");
			}
			finally
			{
				await csvStream.DisposeAsync();
				await writeStream.DisposeAsync();
			}
		});

		return readStream;
	}
}
