import {
  useGetTournamentsQuery,
  useGetPublicTournamentsQuery,
  TournamentViewModel,
} from "../../../store/serviceApi";
import { useCurrentUser } from "../../../CurrentUserContext";

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
  const { isAnonymous, isLoading: isCurrentUserLoading } = useCurrentUser();

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
      skip: !shouldFetchTournaments || isAnonymous,
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
      skip: !shouldFetchTournaments || isAnonymous,
    }
  );

  const {
    data: publicTournamentsData,
    isLoading: isLoadingPublic,
    isError: isErrorPublic,
  } = useGetPublicTournamentsQuery(undefined, {
    skip: !shouldFetchTournaments || !isAnonymous,
  });

  const normalizedSearch = searchTerm.trim().toLowerCase();
  const publicTournaments = (Array.isArray(publicTournamentsData)
    ? publicTournamentsData
    : []) as TournamentViewModel[];
  const filteredPublicTournaments = normalizedSearch
    ? publicTournaments.filter((tournament) => {
        const haystack = [
          tournament.name,
          tournament.description,
          tournament.organizer,
          tournament.country,
          tournament.city,
          tournament.place,
        ]
          .filter((value): value is string => typeof value === "string")
          .join(" ")
          .toLowerCase();

        return haystack.includes(normalizedSearch);
      })
    : publicTournaments;

  // Calculate loading and error states
  const isLoading = isAnonymous
    ? isCurrentUserLoading || isLoadingPublic
    : isCurrentUserLoading || isLoadingAll || isLoadingPaginated;
  const isError = isAnonymous ? isErrorPublic : isErrorAll || isErrorPaginated;

  // Extract data from responses
  const allTournaments = isAnonymous ? filteredPublicTournaments : allTournamentsData?.items || [];
  const paginatedTournaments = isAnonymous
    ? filteredPublicTournaments.slice((currentPage - 1) * DEFAULT_PAGE_SIZE, currentPage * DEFAULT_PAGE_SIZE)
    : paginatedData?.items || [];
  const publicTournamentsFromApi = isAnonymous ? filteredPublicTournaments : [];

  return {
    isAnonymous,
    isLoading,
    isError,
    allTournaments,
    paginatedTournaments,
    publicTournamentsFromApi,
  };
};
