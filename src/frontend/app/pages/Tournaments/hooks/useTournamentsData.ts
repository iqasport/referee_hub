import {
  useGetTournamentsQuery,
  useGetPublicTournamentsQuery,
  useGetCurrentUserQuery,
  TournamentViewModel,
} from "../../../store/serviceApi";

interface UseTournamentsDataReturn {
  isAnonymous: boolean;
  isLoading: boolean;
  isError: boolean;
  allTournaments: TournamentViewModel[];
  paginatedTournaments: TournamentViewModel[];
  publicTournamentsFromApi: TournamentViewModel[];
}

export const useTournamentsData = (
  searchTerm: string,
  currentPage: number,
  DEFAULT_PAGE_SIZE: number
): UseTournamentsDataReturn => {
  // Query current user
  const { currentData: currentUser, isLoading: isCurrentUserLoading, isError: isCurrentUserError } =
    useGetCurrentUserQuery();
  const isAnonymous = !isCurrentUserLoading && (isCurrentUserError || !currentUser);

  // Query tournaments
  const shouldUseAuthenticatedQueries = !isCurrentUserLoading && !isAnonymous;

  // Query for user's private tournaments (no pagination)
  const {
    data: allTournamentsData,
    isLoading: isLoadingAll,
    isError: isErrorAll,
  } = useGetTournamentsQuery(
    {
      filter: searchTerm || undefined,
      skipPaging: true,
    },
    {
      skip: !shouldUseAuthenticatedQueries,
    }
  );

  // Query for public tournaments with pagination
  const {
    data: paginatedData,
    isLoading: isLoadingPaginated,
    isError: isErrorPaginated,
  } = useGetTournamentsQuery(
    {
      filter: searchTerm || undefined,
      page: currentPage,
      pageSize: DEFAULT_PAGE_SIZE,
    },
    {
      skip: !shouldUseAuthenticatedQueries,
    }
  );

  // Query for public tournaments (anonymous users)
  const {
    data: publicTournamentData,
    isLoading: isLoadingPublic,
    isError: isErrorPublic,
  } = useGetPublicTournamentsQuery(undefined, {
    skip: !isAnonymous,
  });

  // Calculate loading and error states
  const isLoading = isCurrentUserLoading || isLoadingAll || isLoadingPaginated || isLoadingPublic;
  const isError = shouldUseAuthenticatedQueries
    ? (isErrorAll || isErrorPaginated)
    : isErrorPublic;

  // Extract data from responses
  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];
  const publicTournamentsFromApi = publicTournamentData || [];

  return {
    isAnonymous,
    isLoading,
    isError,
    allTournaments,
    paginatedTournaments,
    publicTournamentsFromApi,
  };
};
