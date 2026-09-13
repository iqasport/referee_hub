import { useMemo } from "react";
import { TournamentViewModel } from "../../../store/serviceApi";
import { applyRecencyFilter, applyTypeFilter } from "../utils/tournamentUtils";

const DEFAULT_PAGE_SIZE = 20;

interface FilteredTournamentsState {
  filteredAllTournaments: TournamentViewModel[];
  filteredPaginatedTournaments: TournamentViewModel[];
}

export const useFilteredTournaments = (
  isAnonymous: boolean,
  currentPage: number,
  typeFilter: string,
  showOlderTournaments: boolean,
  publicTournamentsFromApi: TournamentViewModel[],
  allTournaments: TournamentViewModel[],
  paginatedTournaments: TournamentViewModel[]
): FilteredTournamentsState => {
  return useMemo(() => {
    // First filter all tournaments by type
    let allFiltered: TournamentViewModel[] = isAnonymous ? publicTournamentsFromApi : allTournaments;
    allFiltered = applyTypeFilter(allFiltered, typeFilter);
    allFiltered = applyRecencyFilter(allFiltered, showOlderTournaments);

    // Then paginate the public tournaments
    if (isAnonymous) {
      const startIndex = (currentPage - 1) * DEFAULT_PAGE_SIZE;
      const endIndex = startIndex + DEFAULT_PAGE_SIZE;
      let filteredPublic = applyTypeFilter(publicTournamentsFromApi, typeFilter);
      filteredPublic = applyRecencyFilter(filteredPublic, showOlderTournaments);
      const paginatedPublic = filteredPublic.slice(startIndex, endIndex);
      return {
        filteredAllTournaments: allFiltered,
        filteredPaginatedTournaments: paginatedPublic,
      };
    }

    let filteredPaginated = applyTypeFilter(paginatedTournaments, typeFilter);
    filteredPaginated = applyRecencyFilter(filteredPaginated, showOlderTournaments);
    return {
      filteredAllTournaments: allFiltered,
      filteredPaginatedTournaments: filteredPaginated,
    };
  }, [
    isAnonymous,
    currentPage,
    typeFilter,
    showOlderTournaments,
    publicTournamentsFromApi,
    allTournaments,
    paginatedTournaments,
  ]);
};
