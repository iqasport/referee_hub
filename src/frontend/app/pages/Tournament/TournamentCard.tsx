import React from "react";

interface TournamentCardProps {
  title: string;
  description: string;
}

const TournamentCard: React.FC<TournamentCardProps> = ({ title, description }) => {
  return (
    <div className="rounded-lg bg-white shadow-lg overflow-hidden">
      <figure className="w-full h-64 bg-gray-300"></figure>
      <h2 className="text-2xl font-bold text-navy-blue p-4">{title}</h2>
      <p className="text-gray-600 p-4">{description}</p>
    </div>
  );
};

export default TournamentCard;
