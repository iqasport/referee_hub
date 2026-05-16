using ManagementHub.Models.Enums;

namespace ManagementHub.Storage.Collections;

/// <summary>
/// Tournament-specific query parameters.
/// </summary>
public class TournamentFilteringParameters : FilteringParameters
{
	/// <summary>
	/// Optional filter by tournament type.
	/// </summary>
	public TournamentType? TournamentTypeFilter { get; set; }
}
