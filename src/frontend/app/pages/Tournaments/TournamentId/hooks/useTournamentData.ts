import {
  useGetTournamentQuery,
  useGetPublicTournamentQuery,
} from "../../../../store/serviceApi";

export const useTournamentData = (tournamentId: string | undefined, isAnonymous: boolean, isCurrentUserLoading: boolean) => {
  const shouldUseAuthenticatedTournamentQuery = !isCurrentUserLoading && !isAnonymous;

  // Query for authenticated tournament view
  const {
    data: authenticatedTournament,
    isLoading: isLoadingAuthenticatedTournament,
    isError: isAuthenticatedTournamentError,
  } = useGetTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || !shouldUseAuthenticatedTournamentQuery }
  );

  // Query for public tournament view (anonymous users)
  const {
    data: publicTournament,
    isLoading: isLoadingPublicTournament,
    isError: isPublicTournamentError,
  } = useGetPublicTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || shouldUseAuthenticatedTournamentQuery }
  );

  const tournament = isAnonymous
    ? (publicTournament ? { ...publicTournament, isCurrentUserInvolved: false } : undefined)
    : authenticatedTournament;

  const isLoading = isCurrentUserLoading || isLoadingAuthenticatedTournament || isLoadingPublicTournament;
  const isError = isAnonymous ? isPublicTournamentError : isAuthenticatedTournamentError;

  return {
    tournament,
    isLoading,
    isError,
  };
};
