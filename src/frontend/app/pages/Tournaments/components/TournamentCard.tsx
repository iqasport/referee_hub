import React from "react";
import { TournamentType } from "../../../store/serviceApi";
import { LocationIcon } from "../../../components/icons";

interface TournamentCardProps {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  type: TournamentType | undefined;
  country: string;
  location: string;
  bannerImageUrl?: string;
  organizer?: string;
  onClick?: () => void;
}

const getTournamentTypeName = (type?: TournamentType): string => {
  return type || "Unknown";
};

const TournamentCard: React.FC<TournamentCardProps> = ({
  title,
  description,
  startDate,
  endDate,
  type,
  country,
  location,
  bannerImageUrl,
  organizer,
  onClick,
}) => {
  const locationText = [location, country].filter(Boolean).join(", ");
  const typeName = getTournamentTypeName(type);
  const dateText = startDate === endDate ? startDate : `${startDate} - ${endDate}`;

  return (
    <div
      className="rounded-lg bg-green-100 shadow-lg overflow-hidden transition-all duration-300 hover:-translate-y-2 hover:shadow-xl cursor-pointer relative group"
      onClick={onClick}
    >
      <figure className="w-full h-64 bg-gray-300 overflow-hidden">
        <img
          src={bannerImageUrl || `https://placehold.co/600x400?text=${encodeURIComponent(title)}`}
          alt={title}
          className="w-full h-full object-cover"
        />
      </figure>
      <div className="p-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded">
            {typeName}
          </span>
          <span className="text-sm text-gray-500">{dateText}</span>
        </div>
        <h2 className="text-2xl font-bold text-navy-blue mb-2">{title}</h2>
        <p className="text-gray-600 mb-3">{description}</p>
        <div className="flex items-center text-sm text-gray-500">
          <LocationIcon className="w-4 h-4 mr-1" />
          {locationText}
        </div>
        {organizer && <div className="text-xs text-gray-500 mt-2">Organized by: {organizer}</div>}
      </div>
    </div>
  );
};

export default TournamentCard;
