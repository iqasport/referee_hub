namespace Service.API.Test.Settings
{
	public class EmailClientSettings
	{
		/// <summary>
		/// Port to open the SMTP server on - must match the port declared in the locally deployed application.
		/// </summary>
		public int SmtpServerPort { get; set; } = 4025;
	}
}
