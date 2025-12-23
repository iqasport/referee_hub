using ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
using Microsoft.Extensions.DependencyInjection;

namespace ManagementHub.Processing.Domain.Tests.Policies.Extensions;

public static class TestPolicyInjectionExtensions
{
	public static IServiceCollection AddTestPolicies(this IServiceCollection services)
	{
		services.AddScoped<IRefereeEligibilityPolicy, HasRequiredCertificationEligibilityPolicy>();
		services.AddScoped<IRefereeEligibilityPolicy, RefereeCertifiedEligibilityPolicy>();
		services.AddScoped<IRefereeEligibilityPolicy, NumberOfAttemptsEligibilityPolicy>();
		services.AddScoped<IRefereeEligibilityPolicy, RefereeAttemptEligibilityPolicy>();
		services.AddScoped<IRefereeEligibilityPolicy, PaymentEligibilityPolicy>();

		services.AddScoped<RefereeEligibilityChecker>();

		return services;
	}
}
