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
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const DEFAULT_PAGE_SIZE = 20;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);
  const { currentData: currentUser, isLoading: isCurrentUserLoading, isError: isCurrentUserError } =
    useGetCurrentUserQuery();
  const isAnonymous = !isCurrentUserLoading && (isCurrentUserError || !currentUser);
  const shouldUseAuthenticatedQueries = !isCurrentUserLoading && !isAnonymous;

  // Query for user's private tournaments (no pagination - uses lazy loading in carousel)
  const {
    data: allTournamentsData,
    isLoading: isLoadingAll,
    isError: isErrorAll,
  } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    skipPaging: true,
  }, {
    skip: !shouldUseAuthenticatedQueries,
  });

  // Query for public tournaments with pagination
  const {
    data: paginatedData,
    isLoading: isLoadingPaginated,
    isError: isErrorPaginated,
  } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    page: currentPage,
    pageSize: DEFAULT_PAGE_SIZE,
  }, {
    skip: !shouldUseAuthenticatedQueries,
  });

  const {
    data: publicTournamentData,
    isLoading: isLoadingPublic,
    isError: isErrorPublic,
  } = useGetPublicTournamentsQuery(undefined, {
    skip: !isAnonymous,
  });

  const isLoading = isCurrentUserLoading || isLoadingAll || isLoadingPaginated || isLoadingPublic;
  const isError = shouldUseAuthenticatedQueries
    ? (isErrorAll || isErrorPaginated)
    : isErrorPublic;

  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];
  const publicTournamentsFromApi = publicTournamentData || [];

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

  // Type filtering is applied client-side since the API doesn't support it yet
  // Note: For better performance, type filtering should be added to the API
  const filteredAllTournaments = useMemo(() => {
    if (isAnonymous) {
      return publicTournamentsFromApi;
    }
    if (!typeFilter) {
      return allTournaments;
    }
    return allTournaments.filter((t) => t.type === typeFilter);
  }, [isAnonymous, publicTournamentsFromApi, allTournaments, typeFilter]);

  const filteredPaginatedTournaments = useMemo(() => {
    if (isAnonymous) {
      const startIndex = (currentPage - 1) * DEFAULT_PAGE_SIZE;
      const endIndex = startIndex + DEFAULT_PAGE_SIZE;

      if (!typeFilter) {
        return publicTournamentsFromApi.slice(startIndex, endIndex);
      }

      return publicTournamentsFromApi
        .filter((t) => t.type === typeFilter)
        .slice(startIndex, endIndex);
    }

    if (!typeFilter) {
      return paginatedTournaments;
    }
    return paginatedTournaments.filter((t) => t.type === typeFilter);
  }, [isAnonymous, currentPage, typeFilter, publicTournamentsFromApi, paginatedTournaments]);

  const { publicTournaments, privateTournaments, totalCount } = useMemo(() => {
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

    if (isAnonymous) {
      return {
        publicTournaments: filteredPaginatedTournaments.map((t) => convertToDisplayFormat({
          ...t,
          isCurrentUserInvolved: false,
        })),
        privateTournaments: [],
        totalCount: filteredAllTournaments.length,
      };
    }

    // Private tournaments come from the unpaginated query (all tournaments)
    const userInvolvedTournaments = filteredAllTournaments
      .filter((t) => t.isCurrentUserInvolved)
      .map(convertToDisplayFormat);

    // Public tournaments come from the paginated query
    const otherTournaments = filteredPaginatedTournaments
      .filter((t) => !t.isCurrentUserInvolved)
      .map(convertToDisplayFormat);

    // Calculate public tournament count from all tournaments (for correct pagination)
    const publicTournamentCount = filteredAllTournaments.filter(
      (t) => !t.isCurrentUserInvolved
    ).length;

    return {
      publicTournaments: otherTournaments,
      privateTournaments: userInvolvedTournaments,
      totalCount: publicTournamentCount,
    };
  }, [isAnonymous, filteredAllTournaments, filteredPaginatedTournaments]);

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
