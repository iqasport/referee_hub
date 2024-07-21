using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Storage.Contexts.Tests;

public class TestPolicyOverride
{
	public int? MaxAttempts { get; set; }
	public int? ExtraTimePercentage { get; set; }
}

[OptionsContext]
public partial class TestPolicyContext : IOptionsContext
{
	public string? UserId { get; set; }
	public string? TestId { get; set; }
}
