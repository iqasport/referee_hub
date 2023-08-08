using ManagementHub.Storage.Collections;

namespace ManagementHub.Service.Filtering;

public abstract class Filtered
{
	public FilteringMetadata? Metadata { get; set; }
}

public class Filtered<T> : Filtered
{
	public IEnumerable<T> Items { get; }

	public Filtered(IEnumerable<T> items)
	{
		this.Items = items;
	}
}

public static class FilteredExtensions
{
	public static Filtered<T> AsFiltered<T>(this IEnumerable<T> items) => new Filtered<T>(items);
}
