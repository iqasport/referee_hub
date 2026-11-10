import {
  useGetPublicTournamentByIdQuery,
  useGetTournamentQuery,
  TournamentViewModel,
} from "../../../../store/serviceApi";

export const useTournamentData = (tournamentId: string | undefined, isAnonymous: boolean, isCurrentUserLoading: boolean) => {
  const {
    data: authenticatedTournament,
    isLoading: isLoadingAuthenticatedTournament,
    isError: isAuthenticatedTournamentError,
  } = useGetTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isCurrentUserLoading || isAnonymous }
  );

  const {
    data: publicTournamentData,
    isLoading: isLoadingPublicTournament,
    isError: isPublicTournamentError,
  } = useGetPublicTournamentByIdQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isCurrentUserLoading || !isAnonymous }
  );

  const tournament = isAnonymous
    ? publicTournamentData as unknown as TournamentViewModel | undefined
    : authenticatedTournament;

  const isLoading = isCurrentUserLoading || (
    isAnonymous ? isLoadingPublicTournament : isLoadingAuthenticatedTournament
  );
  const isError = isAnonymous ? isPublicTournamentError : isAuthenticatedTournamentError;

  return {
    tournament,
    isLoading,
    isError,
  };
};
