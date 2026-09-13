import { useEffect, useState } from "react";
import {
  useGetTournamentsQuery,
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

interface QueryState {
  isCurrentUserLoading: boolean;
  isLoadingAll: boolean;
  isLoadingPaginated: boolean;
  isLoadingPublic: boolean;
  isErrorAll: boolean;
  isErrorPaginated: boolean;
  isErrorPublic: boolean;
}

const getSearchText = (tournament: TournamentViewModel): string => {
  return [
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
};

const getFilteredPublicTournaments = (
  publicTournamentsData: unknown,
  searchTerm: string
): TournamentViewModel[] => {
  const normalizedSearch = searchTerm.trim().toLowerCase();
  const publicTournaments = (Array.isArray(publicTournamentsData)
    ? publicTournamentsData
    : []) as TournamentViewModel[];

  if (!normalizedSearch) {
    return publicTournaments;
  }

  return publicTournaments.filter((tournament) => getSearchText(tournament).includes(normalizedSearch));
};

const getAnonymousPage = (
  tournaments: TournamentViewModel[],
  currentPage: number,
  pageSize: number
): TournamentViewModel[] => {
  const pageStart = (currentPage - 1) * pageSize;
  const pageEnd = currentPage * pageSize;
  return tournaments.slice(pageStart, pageEnd);
};

const getLoadingState = (isAnonymous: boolean, state: QueryState): boolean => {
  return isAnonymous
    ? state.isCurrentUserLoading || state.isLoadingPublic
    : state.isCurrentUserLoading || state.isLoadingAll || state.isLoadingPaginated;
};

const getErrorState = (isAnonymous: boolean, state: QueryState): boolean => {
  return isAnonymous ? state.isErrorPublic : state.isErrorAll || state.isErrorPaginated;
};

export const useTournamentsData = (
  searchTerm: string,
  currentPage: number,
  DEFAULT_PAGE_SIZE: number
): UseTournamentsDataReturn => {
  const { isAnonymous, isLoading: isCurrentUserLoading } = useCurrentUser();
  const [publicTournamentsData, setPublicTournamentsData] = useState<TournamentViewModel[]>([]);
  const [isLoadingPublic, setIsLoadingPublic] = useState(false);
  const [isErrorPublic, setIsErrorPublic] = useState(false);

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

  useEffect(() => {
    const controller = new AbortController();

    if (!shouldFetchTournaments || !isAnonymous) {
      return undefined;
    }

    const loadPublicTournaments = async () => {
      setIsLoadingPublic(true);
      setIsErrorPublic(false);

      try {
        const response = await fetch("/api/v2/public/tournaments", {
          signal: controller.signal,
        });

        if (!response.ok) {
          throw new Error(`Failed to fetch public tournaments: ${response.status}`);
        }

        const data: unknown = await response.json();
        setPublicTournamentsData(Array.isArray(data) ? (data as TournamentViewModel[]) : []);
      } catch (error) {
        if ((error as { name?: string }).name !== "AbortError") {
          setIsErrorPublic(true);
          setPublicTournamentsData([]);
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoadingPublic(false);
        }
      }
    };

    loadPublicTournaments();

    return () => {
      controller.abort();
    };
  }, [isAnonymous, shouldFetchTournaments]);

  const filteredPublicTournaments = getFilteredPublicTournaments(publicTournamentsData, searchTerm);

  const queryState: QueryState = {
    isCurrentUserLoading,
    isLoadingAll,
    isLoadingPaginated,
    isLoadingPublic,
    isErrorAll,
    isErrorPaginated,
    isErrorPublic,
  };

  // Calculate loading and error states
  const isLoading = getLoadingState(isAnonymous, queryState);
  const isError = getErrorState(isAnonymous, queryState);

  // Extract data from responses
  const allTournaments = isAnonymous ? filteredPublicTournaments : allTournamentsData?.items || [];
  const paginatedTournaments = isAnonymous
    ? getAnonymousPage(filteredPublicTournaments, currentPage, DEFAULT_PAGE_SIZE)
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
