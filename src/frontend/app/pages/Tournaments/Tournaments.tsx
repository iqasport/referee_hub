import React, { useRef, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import {
  useGetTournamentsQuery,
  useGetPublicTournamentsQuery,
  useGetCurrentUserQuery,
  TournamentViewModel,
} from "../../store/serviceApi";
import TournamentSection from "./components/TournamentsSection";
import { useFilteredTournaments } from "./hooks/useFilteredTournaments";
import { useTournamentSections } from "./utils/tournamentUtils";

const DEFAULT_PAGE_SIZE = 20;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

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
            <div className="tournament-empty">No tournaments available.</div>
          )}
        </div>
      )}
    </>
  );
};

export default Tournament;
