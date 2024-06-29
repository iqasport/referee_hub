using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;

public class TestPolicyOverride
{
	public int? MaxAttempts { get; set; }
}

[OptionsContext]
public partial class TestPolicyContext : IOptionsContext
{
	public string? UserId { get; set; }
	public string? TestId { get; set; }
}
