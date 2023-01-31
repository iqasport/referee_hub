namespace ManagementHub.Service.Configuration
{
	/// <summary>
	/// A configuration provider that re-maps environment variables onto more complex configuration paths.
	/// </summary>
	public class KnownEnvironmentVariablesConfigurationProvider : ConfigurationProvider
	{
		/// <summary>
		/// Http hosting port variable coming from Heroku.
		/// </summary>
		public static string? HttpHostingPort = Environment.GetEnvironmentVariable("PORT");

		public override void Load()
		{
			// For each custom environment variable we check if it's set and apply it onto Data property

			if (!string.IsNullOrWhiteSpace(HttpHostingPort))
			{
				this.Data.Add(
					$"{ConfigurationConstants.HostingSection}:Endpoints:Http:Url",
					$"http://localhost:{HttpHostingPort}");
			}
		}

		/// <summary>
		/// Configuration source that add a new provider instance to the configuration system.
		/// </summary>
		public static IConfigurationSource Source { get; } = new ConfigSource();
		private class ConfigSource : IConfigurationSource
		{
			public IConfigurationProvider Build(IConfigurationBuilder builder)
				=> new KnownEnvironmentVariablesConfigurationProvider();
		}
	}
}
