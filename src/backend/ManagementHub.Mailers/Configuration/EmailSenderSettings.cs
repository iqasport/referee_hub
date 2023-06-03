using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementHub.Mailers.Configuration;

internal class EmailSenderSettings
{
	public string SenderEmail { get; set; } = "noreply@iqareferees.org";
	public string? SenderDisplayName { get; set; }
	public string ReplyToEmail { get; set; } = "tech@iqareferees.org";
}
