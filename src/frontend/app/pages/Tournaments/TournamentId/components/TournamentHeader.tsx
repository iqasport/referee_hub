import React from "react";

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
    <header style={{ backgroundColor: '#fff', padding: '0 1.5rem' }}>
      <div style={{ maxWidth: '72rem', margin: '0 auto', width: '100%' }}>
        <div
          style={{ position: 'relative', height: window.innerWidth >= 768 ? '12rem' : '10rem', overflow: 'hidden' }}
        >
          <img
            src={bannerImageUrl || "https://placehold.co/1200x200"}
            alt="Tournament banner"
            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
          />
          {/* Title overlay on image */}
          <div
            style={{
              position: 'absolute',
              bottom: 0,
              left: 0,
              right: 0,
              padding: window.innerWidth >= 768 ? '2rem' : '1.5rem',
              color: '#fff',
              background: 'linear-gradient(to top, rgba(0,0,0,0.8), transparent)'
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
              <h1 style={{ fontSize: window.innerWidth >= 768 ? '2rem' : '1.5rem', fontWeight: 'bold', margin: 0 }}>{name}</h1>
              {isManager && (
                <div style={{ backgroundColor: '#16a34a', padding: '0.25rem 0.75rem', borderRadius: '9999px', fontSize: '0.75rem', fontWeight: '600' }}>
                  Manager View
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default TournamentHeader;
