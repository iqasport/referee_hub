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

		public static string? AwsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
		public static string? AwsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
		public static string? AwsBucket = Environment.GetEnvironmentVariable("AWS_BUCKET");

		public override void Load()
		{
			// For each custom environment variable we check if it's set and apply it onto Data property

			if (!string.IsNullOrWhiteSpace(HttpHostingPort))
			{
				this.Data.Add(
					"Kestrel:Endpoints:Http:Url",
					$"http://localhost:{HttpHostingPort}");
			}

			if (!string.IsNullOrWhiteSpace(AwsAccessKeyId))
			{
				this.Data.Add("AWS:AccessKeyId", AwsAccessKeyId);
			}

			if (!string.IsNullOrWhiteSpace(AwsSecretAccessKey))
			{
				this.Data.Add("AWS:SecretAccessKey", AwsSecretAccessKey);
			}

			if (!string.IsNullOrWhiteSpace(AwsBucket))
			{
				this.Data.Add("AWS:Bucket", AwsBucket);
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
