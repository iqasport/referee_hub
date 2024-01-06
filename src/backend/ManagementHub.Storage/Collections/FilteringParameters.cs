namespace ManagementHub.Storage.Collections;

/// <summary>
/// Common query parameters for collection filtering.
/// </summary>
public class FilteringParameters
{
	/// <summary>
	/// An optional filter - it depends on entity what is being filtered.
	/// </summary>
	public string? Filter { get; set; }

	/// <summary>
	/// Page number.
	/// </summary>
	public int Page { get; set; } = 1;

	/// <summary>
	/// Page size.
	/// </summary>
	public int PageSize { get; set; } = 25;

	/// <summary>
	/// if true, paging is skipped.
	/// </summary>
	public bool? SkipPaging { get; set; }
}
