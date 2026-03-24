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

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [showPast, setShowPast] = useState(false);
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

  const { currentData: currentUser } = useGetCurrentUserQuery();

  // Query for user's private tournaments (no pagination - uses lazy loading in carousel)
  const {
    data: allTournamentsData,
    isLoading: isLoadingAll,
    isError: isErrorAll,
  } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    tournamentType: typeFilter || undefined,
    skipPaging: true,
  });

  // Query for public tournaments with pagination
  const {
    data: paginatedData,
    isLoading: isLoadingPaginated,
    isError: isErrorPaginated,
  } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    tournamentType: typeFilter || undefined,
    page: currentPage,
    pageSize: DEFAULT_PAGE_SIZE,
  });

  const isLoading = isLoadingAll || isLoadingPaginated;
  const isError = isErrorAll || isErrorPaginated;

  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];

  const handleSearch = (term: string) => {
    const params = new URLSearchParams(searchParams);
    if (term.trim()) {
      params.set("q", term.trim());
    } else {
      params.delete("q");
    }
    params.delete("page"); // Reset to first page on search
    setSearchParams(params);
  };

  const handleTypeFilter = (type: string) => {
    const params = new URLSearchParams(searchParams);
    if (type) {
      params.set("type", type);
    } else {
      params.delete("type");
    }
    params.delete("page"); // Reset to first page on filter change
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

  const { publicTournaments, privateTournaments, totalCount } = useMemo(() => {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - PAST_TOURNAMENT_DAYS);
    const isPast = (t: TournamentViewModel) => !!t.endDate && new Date(t.endDate) < cutoffDate;

    const convertToDisplayFormat = (t: TournamentViewModel): TournamentData => ({
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

    // Private tournaments come from the unpaginated query (all tournaments)
    const userInvolvedTournaments = allTournaments
      .filter((t) => t.isCurrentUserInvolved && visible(t))
      .map(convertToDisplayFormat);

    // Public tournaments come from the paginated query
    const otherTournaments = paginatedTournaments
      .filter((t) => !t.isCurrentUserInvolved && visible(t))
      .map(convertToDisplayFormat);

    // Calculate public tournament count from all tournaments (for correct pagination)
    const publicTournamentCount = allTournaments.filter(
      (t) => !t.isCurrentUserInvolved && visible(t)
    ).length;

    return {
      publicTournaments: otherTournaments,
      privateTournaments: userInvolvedTournaments,
      totalCount: publicTournamentCount,
    };
  }, [allTournaments, paginatedTournaments, showPast]);

  const hasActiveFilters = !!(searchTerm || typeFilter);

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
            <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer mt-2">
              <input
                type="checkbox"
                checked={showPast}
                onChange={(e) => setShowPast(e.target.checked)}
                className="rounded"
              />
              Show past tournaments
            </label>
          </div>
          <div className="flex items-center gap-3">
            {!!currentUser && (
              <button onClick={() => modalRef.current?.openAdd()} className="btn btn-primary">
                Add Tournament
              </button>
            )}
          </div>
        </div>

        <AddTournamentModal ref={modalRef} />
      </div>

      {isLoading ? (
        <div className="tournament-loading">Loading tournaments...</div>
      ) : isError ? (
        <div className="tournament-error">Error loading tournaments. Please try again.</div>
      ) : (
        <div className="tournament-page-container">
          {privateTournaments.length > 0 && (
            <TournamentSection
              tournaments={privateTournaments}
              visibility="private"
              layout="carousel"
            />
          )}

          {publicTournaments.length > 0 && (
            <>
              <TournamentSection
                tournaments={publicTournaments}
                visibility="public"
                layout="grid"
              />
              {totalCount > DEFAULT_PAGE_SIZE && (
                <div className="flex justify-center py-4">
                  <Pagination
                    current={currentPage}
                    total={totalCount}
                    onChange={handlePageChange}
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
                  <button className="btn-empty-secondary" onClick={clearFilters}>
                    Clear filters
                  </button>
                )}
                {!!currentUser && (
                  <button className="btn-empty-primary" onClick={() => modalRef.current?.openAdd()}>
                    Create Tournament
                  </button>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </>
  );
};

export default Tournament;
