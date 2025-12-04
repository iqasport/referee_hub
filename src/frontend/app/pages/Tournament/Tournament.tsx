import React from "react";
import TournamentCard from "./TournamentCard";
import tournamentsData from "./tournamentsData.json";
import Search from "./Search";

const Tournament = () => {
  return (
    <>
    <Search/>
      <section className="p-4 grid grid-cols-3 gap-6 mx-auto max-w-[80%]">
        {tournamentsData.map((tournament) => (
          <TournamentCard
            key={tournament.id}
            title={tournament.title}
            description={tournament.description}
            date={tournament.date}
            type={tournament.type}
            location={tournament.location}
            coverImg={tournament.cover_img}
          />
        ))}
      </section>
    </>
  );
};

export default Tournament;
