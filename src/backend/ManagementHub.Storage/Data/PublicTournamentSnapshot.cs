using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class PublicTournamentSnapshot : IIdentifiable
{
	public long Id { get; set; }
	public string Key { get; set; } = null!;
	public string SnapshotJson { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}