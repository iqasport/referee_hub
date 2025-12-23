using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Mailers.Configuration;

internal class SmtpSettings
{
	/// <summary>
	/// DNS of the SMTP server we're going to connect to.
	/// </summary>
	[Required]
	public string? Host { get; set; }

	/// <summary>
	/// SMTP port (locally 25, remotely likely 587).
	/// </summary>
	public int Port { get; set; } = 25;

	public string? Username { get; set; }

	public string? Password { get; set; }

	public bool EnableSsl { get; set; }

	public int TimeoutInMilliseconds { get; set; } = 100_000;
}
