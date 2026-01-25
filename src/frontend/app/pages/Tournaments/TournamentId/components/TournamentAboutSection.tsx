import React from "react";
import ReactMarkdown from "react-markdown";
import { LocationIcon } from "../../../../components/icons";

interface TournamentAboutSectionProps {
  place?: string | null;
  city?: string | null;
  country?: string | null;
  description?: string | null;
}

const TournamentAboutSection: React.FC<TournamentAboutSectionProps> = ({
  place,
  city,
  country,
  description,
}) => {
  const location = [place, city, country].filter(Boolean).join(", ") || "TBD";

  return (
    <div style={{ backgroundColor: '#fff', borderRadius: '0.5rem', border: '1px solid #e5e7eb', padding: '1.25rem', marginBottom: '1rem', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <h2 style={{ fontSize: '1.125rem', fontWeight: 'bold', color: '#111827', marginBottom: '0.875rem' }}>About This Tournament</h2>
      {/* Location */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: '0.5rem', marginTop: '0.75rem' }}>
        <div style={{ width: '1.125rem', height: '1.125rem', color: '#16a34a', marginTop: '0.125rem' }}>
          <LocationIcon />
        </div>
        <div>
          <p style={{ fontSize: '0.8125rem', color: '#374151' }}>{location}</p>
        </div>
      </div>
      <h2 style={{ fontSize: '1.125rem', fontWeight: 'bold', color: '#111827', marginBottom: '0.75rem', marginTop: '0.875rem' }}>Description</h2>
      <div style={{ fontSize: '0.875rem', color: '#374151', lineHeight: '1.625', marginBottom: '0.75rem' }}>
        <ReactMarkdown
          components={{
            // Customize link rendering to open in new tab for security
            a: ({ ...props }) => (
              <a 
                {...props} 
                target="_blank" 
                rel="noopener noreferrer"
                style={{ color: '#2563eb', textDecoration: 'underline' }}
              />
            ),
            // Preserve paragraph styling
            p: ({ ...props }) => (
              <p {...props} style={{ marginBottom: '0.5rem' }} />
            ),
          }}
        >
          {description || 'No description provided.'}
        </ReactMarkdown>
      </div>
    </div>
  );
};

export default TournamentAboutSection;
