import React from "react";

interface TournamentCardProps {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  type: string;
  country: string;
  city: string;
  place: string | null;
  isPrivate: boolean;
}

const TournamentCard: React.FC<TournamentCardProps> = ({
  name,
  description,
  startDate,
  endDate,
  type,
  country,
  city,
  place,
  isPrivate,
}) => {
  const locationText = [place || undefined, city, country].filter(Boolean).join(", ");

  return (
    <div className="rounded-lg bg-white shadow-lg overflow-hidden transition-all duration-300 hover:-translate-y-2 hover:shadow-xl cursor-pointer">
      <figure className="w-full h-64 bg-gray-300 overflow-hidden">
        <img
          src={`https://placehold.co/600x400?text=${encodeURIComponent(name)}`}
          alt={name}
          className="w-full h-full object-cover"
        />
      </figure>
      <div className="p-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded">
            {type}
          </span>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <span>
              {startDate} - {endDate}
            </span>
            {isPrivate && (
              <span className="text-xs font-semibold text-amber-700 bg-amber-100 px-2 py-1 rounded">
                Private
              </span>
            )}
          </div>
        </div>
        <h2 className="text-2xl font-bold text-navy-blue mb-2">{name}</h2>
        <p className="text-gray-600 mb-3">{description}</p>
        <div className="flex items-center text-sm text-gray-500">
          <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path
              fillRule="evenodd"
              d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z"
              clipRule="evenodd"
            />
          </svg>
          {locationText}
        </div>
      </div>
    </div>
  );
};

export default TournamentCard;
