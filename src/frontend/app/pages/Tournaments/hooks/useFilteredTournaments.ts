import { useMemo } from "react";
import { TournamentViewModel } from "../../../store/serviceApi";
import { applyTypeFilter } from "../utils/tournamentUtils";

const DEFAULT_PAGE_SIZE = 20;

interface FilteredTournamentsState {
  filteredAllTournaments: TournamentViewModel[];
  filteredPaginatedTournaments: TournamentViewModel[];
}

export const useFilteredTournaments = (
  isAnonymous: boolean,
  currentPage: number,
  typeFilter: string,
  publicTournamentsFromApi: TournamentViewModel[],
  allTournaments: TournamentViewModel[],
  paginatedTournaments: TournamentViewModel[]
): FilteredTournamentsState => {
  return useMemo(() => {
    // First filter all tournaments by type
    let allFiltered: TournamentViewModel[] = isAnonymous ? publicTournamentsFromApi : allTournaments;
    allFiltered = applyTypeFilter(allFiltered, typeFilter);

    // Then paginate the public tournaments
    if (isAnonymous) {
      const startIndex = (currentPage - 1) * DEFAULT_PAGE_SIZE;
      const endIndex = startIndex + DEFAULT_PAGE_SIZE;
      const filteredPublic = applyTypeFilter(publicTournamentsFromApi, typeFilter);
      const paginatedPublic = filteredPublic.slice(startIndex, endIndex);
      return {
        filteredAllTournaments: allFiltered,
        filteredPaginatedTournaments: paginatedPublic,
      };
    }

    const filteredPaginated = applyTypeFilter(paginatedTournaments, typeFilter);
    return {
      filteredAllTournaments: allFiltered,
      filteredPaginatedTournaments: filteredPaginated,
    };
  }, [isAnonymous, currentPage, typeFilter, publicTournamentsFromApi, allTournaments, paginatedTournaments]);
};
