import React, { useRef, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import { useGetTournamentsQuery, TournamentType, TournamentViewModel } from "../../store/serviceApi";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

const getTournamentTypeName = (type?: TournamentType): string => {
  if (!type) return "Unknown";
  return type;
};

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const modalRef = useRef<AddTournamentModalRef>(null);

  //RTK Query hooks
  const { data, isLoading, isError } = useGetTournamentsQuery({});
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
    const originalTournament = tournaments.find(t => t.id === tournament.id.toString());
    if (originalTournament) {
      const modalData = convertToModalFormat(originalTournament);
      modalRef.current?.openEdit(modalData);
    }
  }

  const handleSearch = (term: string) => {
    const params: Record<string, string> = {};
    if (term.trim()) params.q = term.trim();
    if (typeFilter) params.type = typeFilter;
    setSearchParams(params);
  };

  const handleTypeFilter = (type: string) => {
    const params: Record<string, string> = {};
    if (searchTerm.trim()) params.q = searchTerm.trim();
    if (type) params.type = type;
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

  const { publicTournaments, privateTournaments } = useMemo(() => {
    const convertToDisplayFormat = (t: TournamentViewModel): TournamentData => ({
      id: parseInt(t.id || "0"),
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
    return {
      publicTournaments: withFlags.filter((t) => !t.isPrivate),
      privateTournaments: withFlags.filter((t) => t.isPrivate),
    };
  }, [filteredTournaments]);

  return (
    <>
      <div className="max-w-[80%] mx-auto px-4 py-2">
        <Search onSearch={handleSearch} onTypeFilter={handleTypeFilter} selectedType={typeFilter} />
        <button onClick={() => modalRef.current?.openAdd()}>
          Add Tournament
        </button>

        <AddTournamentModal ref={modalRef} />
      </div>
      
      {isLoading ? (
        <div className="text-center text-gray-600 py-8">Loading tournaments...</div>
      ) : isError ? (
        <div className="text-center text-red-600 py-8">Error loading tournaments. Please try again.</div>
      ) : (
        <div className="max-w-[80%] mx-auto px-4 space-y-10">
          {privateTournaments.length > 0 && (
            <TournamentSection
              tournaments={privateTournaments}
              visibility="private"
              onEdit={handleEdit}
            />
          )}

          {publicTournaments.length > 0 && (
            <TournamentSection
              tournaments={publicTournaments}
              visibility="public"
              onEdit={handleEdit}
            />
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
