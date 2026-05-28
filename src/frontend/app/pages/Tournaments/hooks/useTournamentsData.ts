import {
  useGetTournamentsQuery,
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

  // Wait until current-user state is resolved before querying tournaments.
  const shouldFetchTournaments = !isCurrentUserLoading;

  // Query all tournaments without paging.
  // For anonymous users this returns the public list; for authenticated users this includes
  // all visible tournaments with involvement flags used by downstream filtering.
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
      skip: !shouldFetchTournaments,
    }
  );

  // Query tournaments with pagination for list view.
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
      skip: !shouldFetchTournaments,
    }
  );

  // Calculate loading and error states
  const isLoading = isCurrentUserLoading || isLoadingAll || isLoadingPaginated;
  const isError = isErrorAll || isErrorPaginated;

  // Extract data from responses
  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];
  const publicTournamentsFromApi = isAnonymous ? allTournaments : [];

  return {
    isAnonymous,
    isLoading,
    isError,
    allTournaments,
    paginatedTournaments,
    publicTournamentsFromApi,
  };
};
