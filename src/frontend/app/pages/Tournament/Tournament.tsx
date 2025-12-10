import React, { useMemo, useState } from "react";
import tournamentsData from "./tournamentsData.json";
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
  const [searchTerm, setSearchTerm] = useState("");
  const tournaments = useMemo(() => tournamentsData as TournamentData[], []);

  const filteredTournaments = useMemo(() => {
    if (!searchTerm.trim()) {
      return tournaments;
    }

    const lowerTerm = searchTerm.toLowerCase();
    return tournaments.filter((t) => {
      const typeName = getTournamentTypeName(t.type);
      return (
        t.title.toLowerCase().includes(lowerTerm) ||
        t.location.toLowerCase().includes(lowerTerm) ||
        typeName.toLowerCase().includes(lowerTerm)
      );
    });
  }, [tournaments, searchTerm]);

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
        <Search onSearch={setSearchTerm} />
      </div>
      <div className="max-w-[80%] mx-auto px-4 space-y-10">
        <TournamentSection tournaments={privateTournaments} visibility="private" />

        <TournamentSection tournaments={publicTournaments} visibility="public" />
      </div>
    </>
  );
};

export default Tournament;
