import React from "react";
import { Link } from "react-router-dom";

interface TournamentNavBarProps {
  isManager?: boolean;
  onEdit?: () => void;
}

const TournamentNavBar: React.FC<TournamentNavBarProps> = ({ isManager = false, onEdit }) => {
  return (
    <div className="bg-white px-6 py-3 border-b border-gray-200 flex justify-between items-center">
      <Link to="/tournaments" className="text-blue-600 font-medium">
        ← Back to Tournaments
      </Link>
      {isManager && onEdit && (
        <button onClick={onEdit} className="text-blue-600 font-medium">
          Edit Tournament
        </button>
      )}
    </div>
  );
};

export default TournamentNavBar;
