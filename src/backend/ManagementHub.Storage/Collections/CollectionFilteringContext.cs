namespace ManagementHub.Storage.Collections;

/// <summary>
/// Context for filtering collection (where appropriate) without involving logic on the service controller level.
/// </summary>
public class CollectionFilteringContext
{
	public FilteringParameters FilteringParameters { get; set; } = new();

	public FilteringMetadata? FilteringMetadata { get; set; }
}
