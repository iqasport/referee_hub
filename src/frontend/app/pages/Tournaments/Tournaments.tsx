import React, { useMemo, useRef } from "react";
import { faCalendarAlt, faList } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useSearchParams } from "react-router-dom";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import TournamentCalendar from "./components/TournamentCalendar";
import { TournamentList } from "./components/TournamentList";
import { CalendarSkeleton } from "./components/TournamentSkeletons";
import { useFilteredTournaments } from "./hooks/useFilteredTournaments";
import { convertToDisplayFormat, useTournamentSections } from "./utils/tournamentUtils";
import { useTournamentsData } from "./hooks/useTournamentsData";

const DEFAULT_PAGE_SIZE = 20;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const viewParam = searchParams.get("view");
  const viewMode: "list" | "calendar" = viewParam === "calendar" ? "calendar" : "list";
  const modalRef = useRef<AddTournamentModalRef>(null);

  const { isAnonymous, isLoading, isError, allTournaments, paginatedTournaments, publicTournamentsFromApi } =
    useTournamentsData(searchTerm, currentPage, DEFAULT_PAGE_SIZE);

  const { filteredAllTournaments, filteredPaginatedTournaments } = useFilteredTournaments(
    isAnonymous,
    currentPage,
    typeFilter,
    publicTournamentsFromApi,
    allTournaments,
    paginatedTournaments
  );

  const { publicTournaments, privateTournaments, totalCount } = useTournamentSections(
    isAnonymous,
    filteredAllTournaments,
    filteredPaginatedTournaments
  );

  const calendarTournaments = useMemo(
    () => filteredAllTournaments.map(convertToDisplayFormat),
    [filteredAllTournaments]
  );

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

  const handlePageChange = (page: number) => {
    const params = new URLSearchParams(searchParams);
    if (page > 1) {
      params.set("page", page.toString());
    } else {
      params.delete("page");
    }
    setSearchParams(params);
  };

  const handleViewModeChange = (mode: "list" | "calendar") => {
    const params = new URLSearchParams(searchParams);
    params.set("view", mode);
    setSearchParams(params);
  };

  return (
    <>
      <div className="tournament-page-container">
        <div className="tournament-page-header">
          <div className="tournament-search-wrapper">
            <Search onSearch={handleSearch} onTypeFilter={handleTypeFilter} selectedType={typeFilter} />
          </div>
          <div className="view-toggle" aria-label="Tournament view toggle">
            <button
              type="button"
              className={`view-toggle-btn${viewMode === "list" ? " active" : ""}`}
              onClick={() => handleViewModeChange("list")}
              aria-label="List view"
              title="List view"
            >
              <FontAwesomeIcon icon={faList} />
            </button>
            <button
              type="button"
              className={`view-toggle-btn${viewMode === "calendar" ? " active" : ""}`}
              onClick={() => handleViewModeChange("calendar")}
              aria-label="Calendar view"
              title="Calendar view"
            >
              <FontAwesomeIcon icon={faCalendarAlt} />
            </button>
          </div>
          {!isAnonymous && (
            <button onClick={() => modalRef.current?.openAdd()} className="btn btn-primary">
              Add Tournament
            </button>
          )}
        </div>

        {!isAnonymous && <AddTournamentModal ref={modalRef} />}
      </div>

      {isLoading && viewMode === "calendar" ? (
        <CalendarSkeleton />
      ) : isLoading ? (
        <div className="tournament-loading">Loading tournaments...</div>
      ) : isError ? (
        <div className="tournament-error">Error loading tournaments. Please try again.</div>
      ) : viewMode === "calendar" ? (
        <TournamentCalendar tournaments={calendarTournaments} />
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
