import React from "react";

interface TournamentCardProps {
  title: string;
  description: string;
  date: string;
  type: string;
  location: string;
  coverImg: string;
}

const TournamentCard: React.FC<TournamentCardProps> = ({
  title,
  description,
  date,
  type,
  location,
  coverImg,
}) => {
  return (
    <div className="rounded-lg bg-white shadow-lg overflow-hidden">
      <figure className="w-full h-64 bg-gray-300 overflow-hidden">
        <img
          src={`https://placehold.co/600x400?text=${encodeURIComponent(title)}`}
          alt={title}
          className="w-full h-full object-cover"
        />
      </figure>
      <div className="p-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded">
            {type}
          </span>
          <span className="text-sm text-gray-500">{date}</span>
        </div>
        <h2 className="text-2xl font-bold text-navy-blue mb-2">{title}</h2>
        <p className="text-gray-600 mb-3">{description}</p>
        <div className="flex items-center text-sm text-gray-500">
          <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path
              fillRule="evenodd"
              d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z"
              clipRule="evenodd"
            />
          </svg>
          {location}
        </div>
      </div>
    </div>
  );
};

export default TournamentCard;
