import React from "react";
import TournementCard from "./TournementCard";

const Tournement = () => {
  const tournaments = Array.from({ length: 8 }, (_, i) => ({
    id: i + 1,
    title: `Tournament ${i + 1}`,
    description: "Tournament description here",
  }));

  return (
    <section className="p-4 grid grid-cols-3 gap-6 mx-auto max-w-[80%]">
      {tournaments.map((tournament) => (
        <TournementCard
          key={tournament.id}
          title={tournament.title}
          description={tournament.description}
        />
      ))}
    </section>
  );
};

export default Tournement;
