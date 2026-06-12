import { useMemo } from "react";
import { TournamentViewModel } from "../../../../store/serviceApi";

interface PermissionsState {
  isTournamentManager: boolean;
}

export const useTournamentPermissions = (
  currentUser: any | undefined,
  managers: any[] | undefined,
  managersError: boolean
): PermissionsState => {
  return useMemo(() => {
    const isTournamentManager = !managersError && currentUser?.userId && managers
      ? managers.some((manager) => manager.id === currentUser.userId)
      : false;

    return {
      isTournamentManager,
    };
  }, [currentUser, managers, managersError]);
};
