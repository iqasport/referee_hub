import React from "react";
import { CalendarIcon, UsersIcon, HomeIcon, ClockIcon } from "../../../../components/icons";
import InfoCard from "../../../../components/InfoCard";

interface TournamentInfoCardsProps {
  formattedDateRange: string;
  organizer?: string | null;
  startDate?: string;
  registrationEndsDate?: string | null;
  teamCount?: number;
  totalParticipantCount?: number;
}

const TournamentInfoCards: React.FC<TournamentInfoCardsProps> = ({
  formattedDateRange,
  organizer,
  startDate,
  registrationEndsDate,
  teamCount,
  totalParticipantCount,
}) => {
  const registrationEndDate = registrationEndsDate
    ? new Date(registrationEndsDate).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      })
    : startDate
    ? new Date(startDate).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      })
    : "TBD";

  // Format participant label with team count and optional total participant count
  const participantLabel = teamCount !== undefined && totalParticipantCount !== undefined
    ? `Teams: ${teamCount} (${totalParticipantCount} people)`
    : teamCount !== undefined
    ? teamCount.toString()
    : "0";

  return (
    <div style={{ 
      display: 'grid', 
      gridTemplateColumns: window.innerWidth >= 1024 ? 'repeat(4, 1fr)' : window.innerWidth >= 640 ? 'repeat(2, 1fr)' : '1fr', 
      gap: '0.75rem', 
      marginBottom: '1.5rem' 
    }}>
      <InfoCard
        icon={<div style={{ width: '1.25rem', height: '1.25rem' }}><CalendarIcon /></div>}
        label="Tournament Date"
        value={formattedDateRange}
      />
      <InfoCard
        icon={<div style={{ width: '1.25rem', height: '1.25rem' }}><UsersIcon /></div>}
        label="Participants"
        value={participantLabel}
      />
      <InfoCard
        icon={<div style={{ width: '1.25rem', height: '1.25rem' }}><HomeIcon /></div>}
        label="Organizer"
        value={organizer || "N/A"}
      />
      <InfoCard
        icon={<div style={{ width: '1.25rem', height: '1.25rem' }}><ClockIcon /></div>}
        label="Registration Ends"
        value={registrationEndDate}
      />
    </div>
  );
};

export default TournamentInfoCards;
