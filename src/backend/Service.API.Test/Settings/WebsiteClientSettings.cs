using System;

namespace Service.API.Test.Settings;

public class WebsiteClientSettings
{
	/// <summary>
	/// Base url of the website, used to configure HttpClient instances.
	/// </summary>
	public Uri? BaseUrl { get; set; }

	/// <summary>
	/// Custom UserAgent to be used when calling the website.
	/// </summary>
	public string UserAgent { get; set; } = "-";
}
