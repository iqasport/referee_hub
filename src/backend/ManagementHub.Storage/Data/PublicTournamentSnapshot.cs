using System;

namespace ManagementHub.Models.Data;

public partial class PublicTournamentSnapshot
{
	public string Key { get; set; } = null!;
	public string SnapshotJson { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}