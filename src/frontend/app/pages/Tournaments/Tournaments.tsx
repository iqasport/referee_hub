import React, { useRef, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import { useGetTournamentsQuery, TournamentViewModel } from "../../store/serviceApi";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const DEFAULT_PAGE_SIZE = 8;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

  // Query for user's private tournaments (no pagination - uses lazy loading in carousel)
  const {
    data: allTournamentsData,
    isLoading: isLoadingAll,
    isError: isErrorAll,
  } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    skipPaging: true,
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
  });

  const isLoading = isLoadingAll || isLoadingPaginated;
  const isError = isErrorAll || isErrorPaginated;

  const allTournaments = allTournamentsData?.items || [];
  const paginatedTournaments = paginatedData?.items || [];
  const serverTotalCount = paginatedData?.metadata?.totalCount ?? 0;

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
    if (!typeFilter) {
      return allTournaments;
    }
    return allTournaments.filter((t) => t.type === typeFilter);
  }, [allTournaments, typeFilter]);

  const filteredPaginatedTournaments = useMemo(() => {
    if (!typeFilter) {
      return paginatedTournaments;
    }
    return paginatedTournaments.filter((t) => t.type === typeFilter);
  }, [paginatedTournaments, typeFilter]);

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
  }, [filteredAllTournaments, filteredPaginatedTournaments]);

  return (
    <>
      <div className="p-4 mx-auto" style={{ maxWidth: "80%" }}>
        <Search onSearch={handleSearch} onTypeFilter={handleTypeFilter} selectedType={typeFilter} />
        <button onClick={() => modalRef.current?.openAdd()}>Add Tournament</button>

        <AddTournamentModal ref={modalRef} />
      </div>

      {isLoading ? (
        <div className="text-center text-gray-600 py-8">Loading tournaments...</div>
      ) : isError ? (
        <div className="text-center text-red-600 py-8">
          Error loading tournaments. Please try again.
        </div>
      ) : (
        <div className="p-4 mx-auto space-y-10" style={{ maxWidth: "80%" }}>
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
            <div className="text-center text-gray-600 py-8">No tournaments available.</div>
          )}
        </div>
      )}
    </>
  );
};

export default Tournament;
