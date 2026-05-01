import { useMemo } from "react";

interface RosterStats {
  teamCount: number;
  totalParticipantCount: number;
  totalPlayerCount: number;
}

export const useRosterStats = (participants: any[] | undefined): RosterStats => {
  return useMemo(() => {
    if (!participants) {
      return {
        teamCount: 0,
        totalParticipantCount: 0,
        totalPlayerCount: 0,
      };
    }

    const teamCount = participants.length;

    const totalParticipantCount = participants.reduce((total, team) => {
      const playerCount = team.players?.length || 0;
      const coachCount = team.coaches?.length || 0;
      const staffCount = team.staff?.length || 0;
      return total + playerCount + coachCount + staffCount;
    }, 0);

    const totalPlayerCount = participants.reduce((total, team) => {
      const playerCount = team.players?.length || 0;
      return total + playerCount;
    }, 0);

    return {
      teamCount,
      totalParticipantCount,
      totalPlayerCount,
    };
  }, [participants]);
};
