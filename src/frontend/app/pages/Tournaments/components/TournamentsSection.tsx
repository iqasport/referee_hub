import React from "react";
import TournamentCard from "./TournamentCard";
import { TournamentType } from "../../../store/serviceApi";
import { useNavigate } from "../../../utils/navigationUtils";

export type Manager = { id: string; name: string };

export type TournamentData = {
  id: number;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  type: TournamentType | undefined;
  country: string;
  location: string;
  bannerImageUrl?: string;
  organizer?: string;
  managers?: Manager[];
  isPrivate: boolean;
};

type TournamentSectionProps = {
  tournaments: TournamentData[];
  visibility?: "public" | "private";
  onEdit?: (tournament: TournamentData) => void;
};

const TournamentSection: React.FC<TournamentSectionProps> = ({
  tournaments,
  visibility,
  onEdit,
}) => {
  const navigate = useNavigate();

  if (tournaments.length === 0) {
    return (
      <section className="max-w-[80%] mx-auto p-4 text-gray-600">No tournaments available.</section>
    );
  }

  return (
    <section>
      {visibility && (
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          {visibility === "public" ? "Public" : "Your"} Tournaments
        </h2>
      )}
      <div className="p-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mx-auto max-w-[80%]">
        {tournaments.map((tournament) => (
          <TournamentCard
            key={tournament.id}
            title={tournament.title}
            description={tournament.description}
            startDate={tournament.startDate}
            endDate={tournament.endDate}
            type={tournament.type}
            country={tournament.country}
            location={tournament.location}
            bannerImageUrl={tournament.bannerImageUrl}
            organizer={tournament.organizer}
            onEdit={onEdit ? () => onEdit(tournament) : undefined}
            onClick={() => {
              navigate(`/tournaments/${tournament.id}`);
            }}
          />
        ))}
      </div>
    </section>
  );
};

export default TournamentSection;
