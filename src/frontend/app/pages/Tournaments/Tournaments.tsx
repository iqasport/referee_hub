import React, { useRef, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import {
  useGetTournamentsQuery,
  TournamentType,
  TournamentViewModel,
} from "../../store/serviceApi";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const DEFAULT_PAGE_SIZE = 9;

const getTournamentTypeName = (type?: TournamentType): string => {
  if (!type) return "Unknown";
  return type;
};

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const currentPage = parseInt(searchParams.get("page") || "1", 10);
  const modalRef = useRef<AddTournamentModalRef>(null);

  //RTK Query hooks - fetch all tournaments, pagination applied client-side to public only
  const { data, isLoading, isError } = useGetTournamentsQuery({
    filter: searchTerm || undefined,
    skipPaging: true,
  });
  const tournaments = data?.items || [];

  // Convert TournamentViewModel to modal format
  const convertToModalFormat = (tournament: TournamentViewModel) => {
    return {
      id: tournament.id || "",
      name: tournament.name || "",
      description: tournament.description || "",
      startDate: tournament.startDate || "",
      endDate: tournament.endDate || "",
      type: tournament.type || ("" as const),
      country: tournament.country || "",
      city: tournament.city || "",
      place: tournament.place || "",
      organizer: tournament.organizer || "",
      isPrivate: tournament.isPrivate || false,
    };
  };

  function handleEdit(tournament: TournamentData) {
    // Find the original TournamentViewModel from the tournaments array
    const originalTournament = tournaments.find((t) => t.id === tournament.id.toString());
    if (originalTournament) {
      const modalData = convertToModalFormat(originalTournament);
      modalRef.current?.openEdit(modalData);
    }
  }

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

  const filteredTournaments = useMemo(() => {
    let result = tournaments;

    if (typeFilter) {
      result = result.filter((t) => t.type === typeFilter);
    }

    if (searchTerm.trim()) {
      const lowerTerm = searchTerm.toLowerCase();
      result = result.filter((t) => {
        const typeName = getTournamentTypeName(t.type);
        const location = [t.place, t.city, t.country].filter(Boolean).join(", ");
        return (
          (t.name || "").toLowerCase().includes(lowerTerm) ||
          location.toLowerCase().includes(lowerTerm) ||
          typeName.toLowerCase().includes(lowerTerm)
        );
      });
    }

    return result;
  }, [tournaments, searchTerm, typeFilter]);

  const { publicTournaments, privateTournaments, totalPublicCount } = useMemo(() => {
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
      isPrivate: Boolean(t.isPrivate),
    });

    const withFlags = filteredTournaments.map(convertToDisplayFormat);
    const allPublic = withFlags.filter((t) => !t.isPrivate);
    const allPrivate = withFlags.filter((t) => t.isPrivate);

    // Apply client-side pagination to public tournaments only
    const startIndex = (currentPage - 1) * DEFAULT_PAGE_SIZE;
    const paginatedPublic = allPublic.slice(startIndex, startIndex + DEFAULT_PAGE_SIZE);

    return {
      publicTournaments: paginatedPublic,
      privateTournaments: allPrivate,
      totalPublicCount: allPublic.length,
    };
  }, [filteredTournaments, currentPage]);

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
              onEdit={handleEdit}
            />
          )}

          {publicTournaments.length > 0 && (
            <>
              <TournamentSection
                tournaments={publicTournaments}
                visibility="public"
                layout="grid"
                onEdit={handleEdit}
              />
              {totalPublicCount > DEFAULT_PAGE_SIZE && (
                <div className="flex justify-center py-4">
                  <Pagination
                    current={currentPage}
                    total={totalPublicCount}
                    onChange={handlePageChange}
                    pageSize={DEFAULT_PAGE_SIZE}
                    prevIcon={<FontAwesomeIcon icon={faArrowLeft} />}
                    nextIcon={<FontAwesomeIcon icon={faArrowRight} />}
                    className="pagination"
                    hideOnSinglePage={true}
                    jumpPrevIcon={<FontAwesomeIcon icon={faEllipsisH} />}
                    jumpNextIcon={<FontAwesomeIcon icon={faEllipsisH} />}
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
