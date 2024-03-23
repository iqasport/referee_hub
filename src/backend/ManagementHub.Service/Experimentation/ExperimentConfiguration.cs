using Excos.Options.Contextual;
using Excos.Options.Providers.Configuration;

namespace ManagementHub.Service.Experimentation;

public static class ExperimentConfiguration
{
	public static void ConfigureExperiments(this IServiceCollection services)
	{
		services.ConfigureExcosFeatures("Features");
		services.ConfigureExcos<UxExperimentAssignmentOptions>("UX");
		
		services.AddScoped<UxExperimentVariantAssigner>();
		services.AddScoped<AdHocExperimentAssignmentJob>();
	}
}
