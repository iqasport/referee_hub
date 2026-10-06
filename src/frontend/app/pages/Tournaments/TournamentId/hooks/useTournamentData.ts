import {
  useGetTournamentQuery,
} from "../../../../store/serviceApi";

export const useTournamentData = (tournamentId: string | undefined, isAnonymous: boolean, isCurrentUserLoading: boolean) => {
  const shouldFetchTournament = !isCurrentUserLoading;

  // Single tournament endpoint supports both authenticated and anonymous access.
  const {
    data: tournamentData,
    isLoading: isLoadingTournament,
    isError: isTournamentError,
  } = useGetTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || !shouldFetchTournament }
  );

  const tournament = isAnonymous
    ? (tournamentData ? { ...tournamentData, isCurrentUserInvolved: false } : undefined)
    : tournamentData;

  const isLoading = isCurrentUserLoading || isLoadingTournament;
  const isError = isTournamentError;

  return {
    tournament,
    isLoading,
    isError,
  };
};
