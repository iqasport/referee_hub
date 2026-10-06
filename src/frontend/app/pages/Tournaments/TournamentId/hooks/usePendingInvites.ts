import { useMemo } from "react";
import { TournamentInviteViewModel } from "../../../../store/serviceApi";

export const usePendingInvites = (
  invites: TournamentInviteViewModel[] | undefined,
  managedTeamIds: Set<string>
): TournamentInviteViewModel[] => {
  return useMemo(() => {
    if (!invites || managedTeamIds.size === 0) return [];

    return invites.filter((invite) => {
      // Check if this invite is for one of user's teams
      if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;

      // Check if participant approval is pending (user needs to respond)
      return invite.participantApproval?.status === "pending";
    });
  }, [invites, managedTeamIds]);
};
