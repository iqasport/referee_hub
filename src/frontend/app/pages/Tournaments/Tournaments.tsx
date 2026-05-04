import React, { useRef } from "react";
import { useSearchParams } from "react-router-dom";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import { TournamentList } from "./components/TournamentList";
import {
  useGetTournamentsQuery,
  useGetPublicTournamentsQuery,
  useGetCurrentUserQuery,
} from "../../store/serviceApi";
import { useFilteredTournaments } from "./hooks/useFilteredTournaments";
import { useTournamentSections } from "./utils/tournamentUtils";
import { useTournamentsData } from "./hooks/useTournamentsData";

const DEFAULT_PAGE_SIZE = 20;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

  // Load all tournament data with a single hook
  const { isAnonymous, isLoading, isError, allTournaments, paginatedTournaments, publicTournamentsFromApi } =
    useTournamentsData(searchTerm, currentPage, DEFAULT_PAGE_SIZE);

  // Use custom hook for filtering tournaments
  const { filteredAllTournaments, filteredPaginatedTournaments } = useFilteredTournaments(
    isAnonymous,
    currentPage,
    typeFilter,
    publicTournamentsFromApi,
    allTournaments,
    paginatedTournaments
  );

  // Use custom hook to organize tournaments into sections
  const { publicTournaments, privateTournaments, totalCount } = useTournamentSections(
    isAnonymous,
    filteredAllTournaments,
    filteredPaginatedTournaments
  );

  // Handle search
  const handleSearch = (term: string) => {
    const params = new URLSearchParams(searchParams);
    if (term.trim()) {
      params.set("q", term.trim());
    } else {
      params.delete("q");
    }
    params.delete("page");
    setSearchParams(params);
  };

  // Handle type filter
  const handleTypeFilter = (type: string) => {
    const params = new URLSearchParams(searchParams);
    if (type) {
      params.set("type", type);
    } else {
      params.delete("type");
    }
    params.delete("page");
    setSearchParams(params);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    const params = new URLSearchParams(searchParams);
    if (page > 1) {
      params.set("page", page.toString());
    } else {
      params.delete("page");
    }
    setSearchParams(params);
  };

  return (
    <>
      <div className="tournament-page-container">
        <div className="tournament-page-header">
          <div className="tournament-search-wrapper">
            <Search
              onSearch={handleSearch}
              onTypeFilter={handleTypeFilter}
              selectedType={typeFilter}
            />
          </div>
          {!isAnonymous && (
            <button onClick={() => modalRef.current?.openAdd()} className="btn btn-primary">
              Add Tournament
            </button>
          )}
        </div>

        {!isAnonymous && <AddTournamentModal ref={modalRef} />}
      </div>

      {isLoading ? (
        <div className="tournament-loading">Loading tournaments...</div>
      ) : isError ? (
        <div className="tournament-error">Error loading tournaments. Please try again.</div>
      ) : (
        <TournamentList
          isLoading={isLoading}
          isError={isError}
          privateTournaments={privateTournaments}
          publicTournaments={publicTournaments}
          currentPage={currentPage}
          totalCount={totalCount}
          onPageChange={handlePageChange}
        />
      )}
    </>
  );
};

export default Tournament;
