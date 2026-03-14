import React, { useRef, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import { useGetTournamentsQuery, useGetCurrentUserQuery, TournamentViewModel } from "../../store/serviceApi";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const DEFAULT_PAGE_SIZE = 20;

// Tournaments ended more than this many days ago are considered "past"
const PAST_TOURNAMENT_DAYS = 30;

// ── Sub-component ─────────────────────────────────────────────────────────────

interface TournamentGridViewProps {
  privateTournaments: TournamentData[];
  publicTournaments: TournamentData[];
  totalCount: number;
  currentPage: number;
  hasActiveFilters: boolean;
  canAddTournament: boolean;
  onAddTournament: () => void;
  onClearFilters: () => void;
  onPageChange: (page: number) => void;
}
const TournamentGridView: React.FC<TournamentGridViewProps> = ({
  privateTournaments,
  publicTournaments,
  totalCount,
  currentPage,
  hasActiveFilters,
  canAddTournament,
  onAddTournament,
  onClearFilters,
  onPageChange,
}) => (
  <div className="tournament-page-container">
    {privateTournaments.length > 0 && (
      <TournamentSection tournaments={privateTournaments} visibility="private" layout="carousel" />
    )}
    {publicTournaments.length > 0 && (
      <>
        <TournamentSection tournaments={publicTournaments} visibility="public" layout="grid" />
        {totalCount > DEFAULT_PAGE_SIZE && (
          <div className="flex justify-center py-4">
            <Pagination
              current={currentPage}
              total={totalCount}
              onChange={onPageChange}
              pageSize={DEFAULT_PAGE_SIZE}
              prevIcon={<FontAwesomeIcon icon={faArrowLeft} />}
              nextIcon={<FontAwesomeIcon icon={faArrowRight} />}
              className="pagination"
              hideOnSinglePage={true}
              jumpPrevIcon={<FontAwesomeIcon icon={faEllipsisH} />}
              jumpNextIcon={<FontAwesomeIcon icon={faEllipsisH} />}
              showTitle={false}
            />
          </div>
        )}
      </>
    )}
    {privateTournaments.length === 0 && publicTournaments.length === 0 && (
      <div className="tournament-empty-state">
        <div className="tournament-empty-state-icon">🏆</div>
        <h3>{hasActiveFilters ? "No tournaments match your search" : "No upcoming tournaments"}</h3>
        <p>
          {hasActiveFilters
            ? "Try adjusting your search terms or filters."
            : "Tournaments will appear here once they are created."}
        </p>
        <div className="tournament-empty-state-actions">
          {hasActiveFilters && (
            <button className="btn-empty-secondary" onClick={onClearFilters}>
              Clear filters
            </button>
          )}
          {canAddTournament && (
            <button className="btn-empty-primary" onClick={onAddTournament}>
              Create Tournament
            </button>
          )}
        </div>
      </div>
    )}
  </div>
);

// ── useTournamentFilters ──────────────────────────────────────────────────────
// Encapsulates URL search param state and the three handlers that mutate it.

function useTournamentFilters() {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);

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

  const clearFilters = () => setSearchParams(new URLSearchParams());

  return {
    searchTerm,
    typeFilter,
    currentPage,
    handleSearch,
    handleTypeFilter,
    handlePageChange,
    clearFilters,
  };
}

// ── useTournamentData ─────────────────────────────────────────────────────────
// Fetches tournaments and derives display-ready lists from the raw API data.

function useTournamentData(searchTerm: string, typeFilter: string, currentPage: number, showPast: boolean) {
  const {
    data: allTournamentsData,
    isLoading: isLoadingAll,
    isError: isErrorAll,
  } = useGetTournamentsQuery({ filter: searchTerm || undefined, tournamentType: typeFilter || undefined, skipPaging: true });

  const {
    data: paginatedData,
    isLoading: isLoadingPaginated,
    isError: isErrorPaginated,
  } = useGetTournamentsQuery({ filter: searchTerm || undefined, tournamentType: typeFilter || undefined, page: currentPage, pageSize: DEFAULT_PAGE_SIZE });

  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];

  const derived = useMemo(() => {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - PAST_TOURNAMENT_DAYS);
    const isPast = (t: TournamentViewModel) => !!t.endDate && new Date(t.endDate) < cutoffDate;
    const toDisplay = (t: TournamentViewModel): TournamentData => ({
      id: t.id,
      title: t.name || "",
      description: t.description || "",
      startDate: t.startDate || "",
      endDate: t.endDate || "",
      type: t.type,
      country: t.country || "",
      location: [t.place, t.city].filter(Boolean).join(", "),
      bannerImageUrl: t.bannerImageUrl || undefined,
      organizer: t.organizer || undefined,
      isPrivate: Boolean(t.isCurrentUserInvolved),
    });
    const visible = (t: TournamentViewModel) => showPast || !isPast(t);
    return {
      privateTournaments: allTournaments.filter((t) => t.isCurrentUserInvolved && visible(t)).map(toDisplay),
      publicTournaments: paginatedTournaments.filter((t) => !t.isCurrentUserInvolved && visible(t)).map(toDisplay),
      totalCount: allTournaments.filter((t) => !t.isCurrentUserInvolved && visible(t)).length,
      calendarTournaments: allTournaments.filter(visible).map(toDisplay),
    };
  }, [allTournaments, paginatedTournaments, showPast]);

  return {
    ...derived,
    isLoading: isLoadingAll || isLoadingPaginated,
    isError: isErrorAll || isErrorPaginated,
  };
}

// ── TournamentHeader ──────────────────────────────────────────────────────────

interface TournamentHeaderProps {
  typeFilter: string;
  showPast: boolean;
  canAdd: boolean;
  onSearch: (t: string) => void;
  onTypeFilter: (t: string) => void;
  onShowPastChange: (v: boolean) => void;
  onAdd: () => void;
}
const TournamentHeader: React.FC<TournamentHeaderProps> = ({
  typeFilter,
  showPast,
  canAdd,
  onSearch,
  onTypeFilter,
  onShowPastChange,
  onAdd,
}) => (
  <div className="tournament-page-header">
    <div className="tournament-search-wrapper">
      <Search onSearch={onSearch} onTypeFilter={onTypeFilter} selectedType={typeFilter} />
      <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer mt-2">
        <input
          type="checkbox"
          checked={showPast}
          onChange={(e) => onShowPastChange(e.target.checked)}
          className="rounded"
        />
        Show past tournaments
      </label>
    </div>
    <div className="flex items-center gap-3">
      {canAdd && <button onClick={onAdd} className="btn btn-primary">Add Tournament</button>}
    </div>
  </div>
);

// ── Tournament (page root) ────────────────────────────────────────────────────

const Tournament = () => {
  const [showPast, setShowPast] = useState(false);
  const modalRef = useRef<AddTournamentModalRef>(null);
  const { currentData: currentUser } = useGetCurrentUserQuery();

  const {
    searchTerm,
    typeFilter,
    currentPage,
    handleSearch,
    handleTypeFilter,
    handlePageChange,
    clearFilters,
  } = useTournamentFilters();

  const {
    privateTournaments,
    publicTournaments,
    totalCount,
    isLoading,
    isError,
  } = useTournamentData(searchTerm, typeFilter, currentPage, showPast);

  return (
    <>
      <div className="tournament-page-container">
        <TournamentHeader
          typeFilter={typeFilter}
          showPast={showPast}
          canAdd={!!currentUser}
          onSearch={handleSearch}
          onTypeFilter={handleTypeFilter}
          onShowPastChange={setShowPast}
          onAdd={() => modalRef.current?.openAdd()}
        />
        <AddTournamentModal ref={modalRef} />
      </div>

      {isLoading ? (
        <div className="tournament-loading">Loading tournaments...</div>
      ) : isError ? (
        <div className="tournament-error">Error loading tournaments. Please try again.</div>
      ) : (
        <TournamentGridView
          privateTournaments={privateTournaments}
          publicTournaments={publicTournaments}
          totalCount={totalCount}
          currentPage={currentPage}
          hasActiveFilters={!!(searchTerm || typeFilter)}
          canAddTournament={!!currentUser}
          onAddTournament={() => modalRef.current?.openAdd()}
          onClearFilters={clearFilters}
          onPageChange={handlePageChange}
        />
      )}
    </>
  );
};

export default Tournament;
