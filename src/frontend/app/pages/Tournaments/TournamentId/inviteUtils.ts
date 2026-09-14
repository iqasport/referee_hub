import { TournamentInviteViewModel } from "../../../store/serviceApi";

type InviteWithRuntimeFields = TournamentInviteViewModel & {
  participantType?: string;
};

function getParticipantType(invite: TournamentInviteViewModel): string {
  return String((invite as InviteWithRuntimeFields).participantType || "").toLowerCase();
}

export function isRefereeInvite(invite: TournamentInviteViewModel): boolean {
  return getParticipantType(invite) === "referee";
}

export function isTeamInvite(invite: TournamentInviteViewModel): boolean {
  return !isRefereeInvite(invite);
}
