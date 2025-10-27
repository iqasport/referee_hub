using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Service.Areas.User;

/// <summary>
/// Context for loading feature gates for a specific user.
/// </summary>
[OptionsContext]
public partial class FeatureGatesContext : IOptionsContext
{
	public string? UserId { get; set; }
}
