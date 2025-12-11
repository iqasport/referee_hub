import React, { useRef, useMemo, useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import tournamentsData from "./tournamentsData.json";
import AddTournamentModal, { AddTournamentModalRef } from "./components/AddTournamentModal";
import Search from "./components/Search";
import TournamentSection, { TournamentData } from "./components/TournamentsSection";

interface Tournament {
  id?: string;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  type: string;
  country: string;
  city: string;
  place: string;
  isPrivate: boolean;
}

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
  const modalRef = useRef<AddTournamentModalRef>(null);

  // Convert TournamentData to Modal format
  const convertToModalFormat = (tournament: TournamentData) => {
    // Parse location to extract city and place
    const locationParts = tournament.location.split(",").map((s) => s.trim());
    const place = locationParts[0] || "";
    const city = locationParts[1] || "";

    return {
      id: tournament.id.toString(),
      name: tournament.title,
      description: tournament.description,
      startDate: tournament.startDate,
      endDate: tournament.endDate,
      type: getTournamentTypeName(tournament.type),
      country: tournament.country,
      city: city,
      place: place,
      isPrivate: tournament.isPrivate,
    };
  };

  function handleSubmit(tournamentData: Tournament, isEdit: boolean) {
    if (isEdit) {
      console.log("Updating tournament:", tournamentData.id, tournamentData);
      // TODO: Call API - PUT /api/tournaments/{id}
    } else {
      console.log("Creating tournament:", tournamentData);
      // TODO: Call API - POST /api/tournaments
    }
  }

  function handleEdit(tournament: TournamentData) {
    const modalData = convertToModalFormat(tournament);
    modalRef.current?.openEdit(modalData);
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
        <button onClick={() => modalRef.current?.openAdd()}>Add Tournament</button>

        <AddTournamentModal ref={modalRef} onSubmit={handleSubmit} />
      </div>
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
    </>
  );
};

export default Tournament;
