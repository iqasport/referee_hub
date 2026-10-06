import { useGetTournamentManagersQuery, useGetCurrentUserQuery } from "../../../../store/serviceApi";

export const useTournamentManagerCheck = (tournamentId: string | undefined) => {
  const { data: currentUser } = useGetCurrentUserQuery();

  // Check if user has TournamentManager role for this tournament
  const isTournamentManagerOfThis = currentUser?.roles?.some((role: any) => {
    if (role.roleType !== "TournamentManager") return false;
    if (role.tournament === "ANY") return true;
    if (Array.isArray(role.tournament)) {
      return role.tournament.includes(tournamentId);
    }
    return role.tournament === tournamentId;
  });

  const shouldFetchManagers = Boolean(tournamentId && isTournamentManagerOfThis);
  const { data: managers = [], isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

  const isTournamentManager = !managersError && currentUser?.userId && managers
    ? managers.some((manager) => manager.id === currentUser.userId)
    : false;

  return {
    isTournamentManager,
    managers,
    managersError,
  };
};
