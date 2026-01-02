namespace ManagementHub.Models.Configuration;

public class GenderDataRetentionSettings
{
	public int NotUpdatedForMonths { get; set; } = 6;
	public int MonthsSinceLastTournamentEnded { get; set; } = 3;
	public bool EnableAutomaticDeletion { get; set; } = true;
}
