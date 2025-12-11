import React, { useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import tournamentsData from "./tournamentsData.json";
import AddTournamentModal from "./components/AddTournamentModal";
import Search from "./Search";
import TournamentSection, { TournamentData } from "./TournamentSection";

//This is a placeholder until we get the api output, that's why its both string and number for the moment
const getTournamentTypeName = (type: number | string): string => {
  const typeMap: Record<number, string> = {
    0: "Club",
    1: "National",
    2: "Youth",
    3: "Fantasy",
  };
  const key = typeof type === "string" ? parseInt(type, 10) : type;
  return typeMap[key] || "Unknown";
};

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchTerm = searchParams.get("q") || "";
  const typeFilter = searchParams.get("type") || "";
  const tournaments = useMemo(() => tournamentsData as TournamentData[], []);

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
      result = result.filter((t) => t.type === parseInt(typeFilter));
    }

    if (searchTerm.trim()) {
      const lowerTerm = searchTerm.toLowerCase();
      result = result.filter((t) => {
        const typeName = getTournamentTypeName(t.type);
        return (
          t.title.toLowerCase().includes(lowerTerm) ||
          t.location.toLowerCase().includes(lowerTerm) ||
          typeName.toLowerCase().includes(lowerTerm)
        );
      });
    }

    return result;
  }, [tournaments, searchTerm, typeFilter]);

  const { publicTournaments, privateTournaments } = useMemo(() => {
    const withFlags = filteredTournaments.map((t) => ({ ...t, isPrivate: Boolean(t.isPrivate) }));
    return {
      publicTournaments: withFlags.filter((t) => !t.isPrivate),
      privateTournaments: withFlags.filter((t) => t.isPrivate),
    };
  }, [filteredTournaments]);

  return (
    <>
      <div className="max-w-[80%] mx-auto px-4 py-2">
        <Search onSearch={handleSearch} onTypeFilter={handleTypeFilter} selectedType={typeFilter} />
        <AddTournamentModal />
      </div>
      <div className="max-w-[80%] mx-auto px-4 space-y-10">
        {privateTournaments.length > 0 && (
          <TournamentSection tournaments={privateTournaments} visibility="private" />
        )}

        {publicTournaments.length > 0 && (
          <TournamentSection tournaments={publicTournaments} visibility="public" />
        )}

        {privateTournaments.length === 0 && publicTournaments.length === 0 && (
          <div className="text-center text-gray-600 py-8">No tournaments available.</div>
        )}
      </div>
    </>
  );
};

export default Tournament;
