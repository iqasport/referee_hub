import React from "react";
import { Link } from "react-router-dom";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

interface TournamentHeaderProps {
  bannerImageUrl?: string | null;
  name?: string | null;
  isManager?: boolean;
}

const TournamentHeader: React.FC<TournamentHeaderProps> = ({
  bannerImageUrl,
  name,
  isManager = false,
}) => {
  return (
    <header className="tournament-header">
      <div className="tournament-header-wrapper">
        <div className="tournament-header-container">
          <Link to="/tournaments" className="tournament-back-button">
            <FontAwesomeIcon icon={faArrowLeft} />
          </Link>
          <img
            src={bannerImageUrl || "https://placehold.co/1200x200"}
            alt="Tournament banner"
            className="tournament-banner-image"
          />
          {/* Title overlay on image */}
          <div className="tournament-title-overlay">
            <div className="tournament-title-content">
              <h1 className="tournament-title">{name}</h1>
              {isManager && <div className="manager-badge">Manager View</div>}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default TournamentHeader;
