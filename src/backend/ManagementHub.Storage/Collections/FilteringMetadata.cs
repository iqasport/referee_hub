namespace ManagementHub.Storage.Collections;

/// <summary>
/// Additional metadata after the filters are applied.
/// </summary>
public class FilteringMetadata
{
	/// <summary>
	/// Total count of the elements of a collection (after filters are applied but without paging applied).
	/// </summary>
	public int? TotalCount { get; set; }
}
