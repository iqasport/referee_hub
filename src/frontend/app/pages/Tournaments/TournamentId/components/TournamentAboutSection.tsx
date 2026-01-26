import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
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
          remarkPlugins={[remarkGfm]}
          components={{
            // Customize link rendering to open in new tab for security
            // eslint-disable-next-line react/prop-types
            a: (props) => {
              // eslint-disable-next-line react/prop-types
              const { href, children } = props;
              return (
                <a
                  href={href}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{ color: '#2563eb', textDecoration: 'underline' }}
                >
                  {children}
                </a>
              );
            },
            // Headings - sized progressively smaller than Description header (1.125rem)
            // eslint-disable-next-line react/prop-types
            h1: (props) => {
              const { children } = props;
              return <h1 style={{ fontSize: '1rem', fontWeight: 'bold', color: '#111827', marginTop: '1rem', marginBottom: '0.5rem' }}>{children}</h1>;
            },
            // eslint-disable-next-line react/prop-types
            h2: (props) => {
              const { children } = props;
              return <h2 style={{ fontSize: '0.9375rem', fontWeight: 'bold', color: '#111827', marginTop: '0.875rem', marginBottom: '0.5rem' }}>{children}</h2>;
            },
            // eslint-disable-next-line react/prop-types
            h3: (props) => {
              const { children } = props;
              return <h3 style={{ fontSize: '0.875rem', fontWeight: 'bold', color: '#111827', marginTop: '0.75rem', marginBottom: '0.375rem' }}>{children}</h3>;
            },
            // eslint-disable-next-line react/prop-types
            h4: (props) => {
              const { children } = props;
              return <h4 style={{ fontSize: '0.8125rem', fontWeight: 'bold', color: '#374151', marginTop: '0.625rem', marginBottom: '0.375rem' }}>{children}</h4>;
            },
            // eslint-disable-next-line react/prop-types
            h5: (props) => {
              const { children } = props;
              return <h5 style={{ fontSize: '0.75rem', fontWeight: 'bold', color: '#374151', marginTop: '0.5rem', marginBottom: '0.25rem' }}>{children}</h5>;
            },
            // eslint-disable-next-line react/prop-types
            h6: (props) => {
              const { children } = props;
              return <h6 style={{ fontSize: '0.75rem', fontWeight: '600', color: '#6b7280', marginTop: '0.5rem', marginBottom: '0.25rem' }}>{children}</h6>;
            },
            // Lists
            // eslint-disable-next-line react/prop-types
            ul: (props) => {
              const { children } = props;
              return <ul style={{ marginBottom: '0.5rem', paddingLeft: '1.5rem', listStyleType: 'disc' }}>{children}</ul>;
            },
            // eslint-disable-next-line react/prop-types
            ol: (props) => {
              const { children } = props;
              return <ol style={{ marginBottom: '0.5rem', paddingLeft: '1.5rem', listStyleType: 'decimal' }}>{children}</ol>;
            },
            // eslint-disable-next-line react/prop-types
            li: (props) => {
              const { children } = props;
              return <li style={{ marginBottom: '0.25rem' }}>{children}</li>;
            },
            // Preserve paragraph styling
            // eslint-disable-next-line react/prop-types
            p: (props) => {
              const { children } = props;
              return <p style={{ marginBottom: '0.5rem' }}>{children}</p>;
            },
            // Emphasis and strong
            // eslint-disable-next-line react/prop-types
            em: (props) => {
              const { children } = props;
              return <em style={{ fontStyle: 'italic' }}>{children}</em>;
            },
            // eslint-disable-next-line react/prop-types
            strong: (props) => {
              const { children } = props;
              return <strong style={{ fontWeight: 'bold' }}>{children}</strong>;
            },
            // Code blocks
            // eslint-disable-next-line react/prop-types
            code: (props) => {
              const { children } = props;
              return <code style={{ backgroundColor: '#f3f4f6', padding: '0.125rem 0.25rem', borderRadius: '0.25rem', fontSize: '0.8125rem', fontFamily: 'monospace' }}>{children}</code>;
            },
            // eslint-disable-next-line react/prop-types
            pre: (props) => {
              const { children } = props;
              return <pre style={{ backgroundColor: '#f3f4f6', padding: '0.75rem', borderRadius: '0.375rem', marginBottom: '0.5rem', overflowX: 'auto' }}>{children}</pre>;
            },
            // Blockquotes
            // eslint-disable-next-line react/prop-types
            blockquote: (props) => {
              const { children } = props;
              return <blockquote style={{ borderLeft: '4px solid #e5e7eb', paddingLeft: '1rem', marginLeft: '0', marginBottom: '0.5rem', color: '#6b7280', fontStyle: 'italic' }}>{children}</blockquote>;
            },
            // Horizontal rule
            // eslint-disable-next-line react/prop-types
            hr: () => {
              return <hr style={{ border: 'none', borderTop: '1px solid #e5e7eb', marginTop: '0.75rem', marginBottom: '0.75rem' }} />;
            },
          }}
        >
          {description || 'No description provided.'}
        </ReactMarkdown>
      </div>
    </div>
  );
};

export default TournamentAboutSection;
