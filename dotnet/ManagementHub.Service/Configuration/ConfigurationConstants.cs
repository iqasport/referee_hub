namespace ManagementHub.Service.Configuration;

public static class ConfigurationConstants
{
    /// <summary>
    /// Name of the configuration section for Kestrel.
    /// </summary>
    public const string HostingSection = "Hosting";

    public const string AppSettingsFileName = "appsettings.json";

    public const string AppSettingsEnvFileNameFormat = "appsettings.{0}.json";
}
