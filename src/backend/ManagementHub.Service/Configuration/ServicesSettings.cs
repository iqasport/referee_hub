namespace ManagementHub.Service.Configuration;

public class ServicesSettings
{
	public bool UseInMemoryDatabase { get; set; }
	public bool SeedDatabaseWithTestData { get; set; }
	public bool UseInMemoryJobSystem { get; set; }
	public bool UseLocalFilesystemBlobStorage { get; set; }
	public bool UseDebugMailer { get; set; }

	//TODO: move this elsewhere
	public string? RedisConnectionString { get; set; }
}
