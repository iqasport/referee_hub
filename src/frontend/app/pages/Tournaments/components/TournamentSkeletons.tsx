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

// ── CalendarSkeleton ──────────────────────────────────────────────────────────

export const CalendarSkeleton: React.FC = () => (
  <div className="calendar-skeleton">
    {/* Header bar */}
    <div className="cal-skel-header">
      <div className="skel-line skel-cal-nav" />
      <div className="skel-line skel-cal-title" />
      <div className="skel-line skel-cal-nav" />
    </div>
    {/* Day-name row */}
    <div className="cal-skel-grid">
      {Array.from({ length: 7 }).map((_, i) => (
        <div key={i} className="skel-line skel-day-name" />
      ))}
      {/* 5 weeks × 7 days */}
      {Array.from({ length: 35 }).map((_, i) => (
        <div key={i} className="skel-cell" />
      ))}
    </div>
  </div>
);
