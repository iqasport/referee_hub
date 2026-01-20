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
    <div className="tournament-card" onClick={onClick}>
      <figure>
        <img
          src={bannerImageUrl || `https://placehold.co/600x400?text=${encodeURIComponent(title)}`}
          alt={title}
        />
      </figure>
      <div className="card-content">
        <div className="card-header">
          <span className="card-type">{typeName}</span>
          <span className="card-date">{dateText}</span>
        </div>
        <h2 className="card-title">{title}</h2>
        <p className="card-description">{description}</p>
        <div className="card-footer">
          <div className="card-location">
            <LocationIcon />
            <span>{locationText}</span>
          </div>
          {organizer && <div className="card-organizer">Organized by: {organizer}</div>}
        </div>
      </div>
    </div>
  );
};

export default TournamentCard;
