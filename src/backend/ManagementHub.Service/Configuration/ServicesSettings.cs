﻿namespace ManagementHub.Service.Configuration;

public class ServicesSettings
{
	public bool UseInMemoryDatabase { get; set; }
	public bool UseInMemoryJobSystem { get; set; }
	public bool UseLocalFilesystemBlobStorage { get; set; }
	public bool UseDebugMailer { get; set; }
}