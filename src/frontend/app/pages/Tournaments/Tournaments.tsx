import React, { useRef, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import { useGetTournamentsQuery, TournamentViewModel } from "../../store/serviceApi";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const DEFAULT_PAGE_SIZE = 9;

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

  // When type filter is active, we need all tournaments for client-side filtering
  // Otherwise, use server-side pagination for better performance
  const useServerPaging = !typeFilter;

  //RTK Query hooks - conditional pagination based on filtering needs
  const { data, isLoading, isError } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    page: useServerPaging ? currentPage : undefined,
    pageSize: useServerPaging ? DEFAULT_PAGE_SIZE : undefined,
    skipPaging: !useServerPaging,
  });
  const tournaments = data?.items || [];
  const serverTotalCount = data?.metadata?.totalCount ?? 0;

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
  const filteredTournaments = useMemo(() => {
    if (!typeFilter) {
      return tournaments;
    }
    return tournaments.filter((t) => t.type === typeFilter);
  }, [tournaments, typeFilter]);

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

    const withFlags = filteredTournaments.map(convertToDisplayFormat);
    const userInvolvedTournaments = withFlags.filter((t) => t.isPrivate);
    const otherTournaments = withFlags.filter((t) => !t.isPrivate);

    // When using server-side pagination, tournaments are already paginated
    // When type filtering is active, apply client-side pagination
    if (useServerPaging) {
      return {
        publicTournaments: otherTournaments,
        privateTournaments: userInvolvedTournaments,
        totalCount: serverTotalCount,
      };
    }

    // Client-side pagination for filtered results
    const startIndex = (currentPage - 1) * DEFAULT_PAGE_SIZE;
    const paginatedOther = otherTournaments.slice(startIndex, startIndex + DEFAULT_PAGE_SIZE);

    return {
      publicTournaments: paginatedOther,
      privateTournaments: userInvolvedTournaments,
      totalCount: otherTournaments.length,
    };
  }, [filteredTournaments, useServerPaging, serverTotalCount, currentPage]);

  return (
    <>
      <div className="max-w-[80%] mx-auto px-4 py-2">
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
        <div className="max-w-[80%] mx-auto px-4 space-y-10">
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
