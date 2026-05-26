import { useMemo } from "react";

export const useManagedTeamIds = (managedTeamsData: any[] | undefined): Set<string> => {
  return useMemo(() => {
    const teamIds = new Set<string>();
    if (managedTeamsData) {
      managedTeamsData.forEach((team) => {
        if (team.teamId) {
          teamIds.add(team.teamId);
        }
      });
    }
    return teamIds;
  }, [managedTeamsData]);
};
