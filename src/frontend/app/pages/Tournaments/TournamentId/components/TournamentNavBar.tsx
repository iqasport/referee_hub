import React from "react";
import { Link } from "react-router-dom";

interface TournamentNavBarProps {
  isManager?: boolean;
  onEdit?: () => void;
}

const TournamentNavBar: React.FC<TournamentNavBarProps> = ({ isManager = false, onEdit }) => {
  return (
    <div style={{ backgroundColor: '#fff', padding: '0 1.5rem' }}>
      <div style={{ maxWidth: '72rem', margin: '0 auto', width: '100%', padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '0.75rem' }}>
        <Link to="/tournaments" style={{ color: '#16a34a', fontWeight: '500', textDecoration: 'none', fontSize: '0.875rem' }}>
          ← Back to Tournaments
        </Link>
        {isManager && onEdit && (
          <button onClick={onEdit} style={{ color: '#16a34a', fontWeight: '500', background: 'none', border: 'none', cursor: 'pointer', padding: 0, fontSize: '0.875rem' }}>
            Edit Tournament
          </button>
        )}
      </div>
    </div>
  );
};

export default TournamentNavBar;
