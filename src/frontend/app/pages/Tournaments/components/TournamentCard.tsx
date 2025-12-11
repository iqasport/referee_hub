import React from "react";

interface TournamentCardProps {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  type: number;
  country: string;
  location: string;
  bannerImageUrl?: string;
  organizer?: string;
  onEdit?: () => void;
}

const getTournamentTypeName = (type: number): string => {
  const typeMap: { [key: number]: string } = {
    0: "Club",
    1: "National",
    2: "Youth",
    3: "Fantasy",
  };
  return typeMap[type] || "Unknown";
};

const LocationIcon = () => (
  <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
    <path
      fillRule="evenodd"
      d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z"
      clipRule="evenodd"
    />
  </svg>
);

const EditIcon = () => (
  <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      strokeWidth={2}
      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
    />
  </svg>
);

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
  onEdit,
}) => {
  const locationText = [location, country].filter(Boolean).join(", ");
  const typeName = getTournamentTypeName(type);
  const dateText = startDate === endDate ? startDate : `${startDate} - ${endDate}`;

  return (
    <div className="rounded-lg bg-green-100 shadow-lg overflow-hidden transition-all duration-300 hover:-translate-y-2 hover:shadow-xl cursor-pointer relative group">
      {onEdit && (
        <button
          onClick={(e) => {
            e.stopPropagation();
            onEdit();
          }}
         
        >
          <EditIcon />
          Edit
        </button>
      )}
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
          <LocationIcon />
          {locationText}
        </div>
        {organizer && <div className="text-xs text-gray-500 mt-2">Organized by: {organizer}</div>}
      </div>
    </div>
  );
};

export default TournamentCard;
