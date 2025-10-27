using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Feature gates for controlling frontend features.
/// </summary>
public class FeatureGates
{
	/// <summary>
	/// Example test flag for demonstration purposes.
	/// </summary>
	public bool IsTestFlag { get; set; }
}

/// <summary>
/// Context for loading feature gates for a specific user.
/// </summary>
[OptionsContext]
public partial class FeatureGatesContext : IOptionsContext
{
	public string? UserId { get; set; }
}
