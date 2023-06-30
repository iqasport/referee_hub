namespace Service.API.Test.Settings;

public class DatabaseSettings
{
	public string? Host { get; set; }
	public int Port { get; set; } = 5432;
	public string? Database { get; set; }
	public string? UserName { get; set; }
	public string? Password { get; set; }
}
