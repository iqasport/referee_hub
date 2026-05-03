import React from "react";

// ── TournamentCardSkeleton ────────────────────────────────────────────────────

const CardSkeleton: React.FC = () => (
  <div className="tournament-card-skeleton">
    <div className="skel-banner" />
    <div className="skel-body">
      <div className="skel-line skel-title" />
      <div className="skel-line skel-subtitle" />
      <div className="skel-line skel-short" />
    </div>
  </div>
);

interface TournamentCardSkeletonProps {
  count?: number;
}
export const TournamentCardSkeleton: React.FC<TournamentCardSkeletonProps> = ({ count = 8 }) => (
  <div className="tournament-page-container">
    <div className="tournament-section-skeleton">
      <div className="skel-line skel-section-title" />
      <div className="tournament-grid-skeleton">
        {Array.from({ length: count }).map((_, i) => <CardSkeleton key={i} />)}
      </div>
    </div>
  </div>
);
