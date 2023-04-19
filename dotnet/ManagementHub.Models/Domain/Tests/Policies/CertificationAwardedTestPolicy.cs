namespace ManagementHub.Models.Domain.Tests.Policies;

/// <summary>
/// Test policy which upon passing awards the user with the specified certification.
/// </summary>
public record CertificationAwardedTestPolicy(Certification Certification) : ITestPolicy
{
}
