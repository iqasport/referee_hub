import React, { useMemo, useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import tournamentsData from "./tournamentsData.json";
import AddTournamentModal from "./components/AddTournamentModal";
import Search from "./Search";
import TournamentSection, { TournamentData } from "./TournamentSection";

const getTournamentTypeName = (type: number): string => {
  const typeMap: { [key: number]: string } = {
    0: "Club",
    1: "National",
    2: "Youth",
    3: "Fantasy",
  };
  return typeMap[type] || "Unknown";
};

const Tournament = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const querySearch = searchParams.get("q") || "";
  const queryType = searchParams.get("type") || "";
  const [searchTerm, setSearchTerm] = useState(querySearch);
  const [typeFilter, setTypeFilter] = useState(queryType);
  const tournaments = useMemo(() => tournamentsData as TournamentData[], []);

  useEffect(() => {
    setSearchTerm(querySearch);
  }, [querySearch]);

  useEffect(() => {
    setTypeFilter(queryType);
  }, [queryType]);

  const handleSearch = (term: string) => {
    setSearchTerm(term);
    const params: Record<string, string> = {};
    if (term.trim()) params.q = term;
    if (typeFilter) params.type = typeFilter;
    setSearchParams(params);
  };

  const handleTypeFilter = (type: string) => {
    setTypeFilter(type);
    const params: Record<string, string> = {};
    if (searchTerm.trim()) params.q = searchTerm;
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
        <TournamentSection tournaments={privateTournaments} visibility="private" />

        <TournamentSection tournaments={publicTournaments} visibility="public" />
      </div>
    </>
  );
};

export default Tournament;
