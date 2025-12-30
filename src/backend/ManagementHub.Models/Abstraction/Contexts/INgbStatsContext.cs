using System;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface INgbStatsContext
{
	public int TotalRefereesCount { get; }
	public int HeadRefereesCount { get; }
	public int AssistantRefereesCount { get; }
	public int FlagRefereesCount { get; }
	public int ScorekeeperRefereesCount { get; }
	public int UncertifiedRefereesCount { get; }

	public int CompetitiveTeamsCount { get; }
	public int DevelopingTeamsCount { get; }
	public int InactiveTeamsCount { get; }

	public int YouthTeamsCount { get; }
	public int UniversityTeamsCount { get; }
	public int CommunityTeamsCount { get; }
	public int TotalTeamsCount { get; }

	public DateTime CollectedAt { get; }
}
