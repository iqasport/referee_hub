import { useMemo } from "react";
import { TournamentInviteViewModel } from "../../../../store/serviceApi";

interface ApprovedTeam {
  teamId: string;
  teamName: string;
  ngb: string;
}

export const useApprovedTeams = (
  invites: TournamentInviteViewModel[] | undefined,
  managedTeamIds: Set<string>,
  managedTeamsData: any[] | undefined
): ApprovedTeam[] => {
  return useMemo(() => {
    if (!invites || managedTeamIds.size === 0 || !managedTeamsData) return [];

    return invites
      .filter((invite) => {
        // Check if this invite is for one of user's teams
        if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;
        // Check if the invite is fully approved
        return invite.status === "approved";
      })
      .map((invite) => {
        const teamData = managedTeamsData.find((t) => t.teamId === invite.participantId);
        return {
          teamId: invite.participantId,
          teamName: invite.participantName || teamData?.teamName || "Unknown Team",
          ngb: teamData?.ngb || "",
        };
      });
  }, [invites, managedTeamIds, managedTeamsData]);
};
