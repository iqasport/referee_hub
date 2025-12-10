import React from "react";
import TournamentCard from "./TournamentCard";
import tournamentsData from "./tournamentsData.json";
import Search from "./Search";

type TournamentData = {
  Id: number;
  UniqueId: string;
  Name: string;
  Description: string;
  StartDate: string;
  EndDate: string;
  Type: string;
  Country: string;
  City: string;
  Place: string | null;
  IsPrivate: boolean;
  CreatedAt: string;
  UpdatedAt: string;
};

const Tournament = () => {
  const tournaments = tournamentsData as TournamentData[];

  return (
    <>
      <Search />
      <section className="p-4 grid grid-cols-3 gap-6 mx-auto max-w-[80%]">
        {tournaments.map((tournament) => (
          <TournamentCard
            key={tournament.UniqueId}
            name={tournament.Name}
            description={tournament.Description}
            startDate={tournament.StartDate}
            endDate={tournament.EndDate}
            type={tournament.Type}
            country={tournament.Country}
            city={tournament.City}
            place={tournament.Place}
            isPrivate={tournament.IsPrivate}
          />
        ))}
      </section>
    </>
  );
};

export default Tournament;
