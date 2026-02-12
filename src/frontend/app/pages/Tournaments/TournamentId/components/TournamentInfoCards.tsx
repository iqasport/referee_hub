import React from "react";
import { CalendarIcon, UsersIcon, HomeIcon, ClockIcon } from "../../../../components/icons";
import InfoCard from "../../../../components/InfoCard";

interface TournamentInfoCardsProps {
  formattedDateRange: string;
  organizer?: string | null;
  startDate?: string;
  registrationEndsDate?: string | null;
  isRegistrationOpen?: boolean;
  tournamentType?: string | null;
}

const TournamentInfoCards: React.FC<TournamentInfoCardsProps> = ({
  formattedDateRange,
  organizer,
  startDate,
  registrationEndsDate,
  isRegistrationOpen,
  tournamentType,
}) => {
  // Check if registration is closed or if the registration end date has passed
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Reset time to start of day for accurate comparison
  
  let registrationEndDate: string;
  
  if (isRegistrationOpen === false) {
    // If explicitly closed, show "Closed"
    registrationEndDate = "Closed";
  } else if (registrationEndsDate) {
    const regEndDate = new Date(registrationEndsDate);
    regEndDate.setHours(0, 0, 0, 0);
    
    // Check if the date has passed
    if (regEndDate < today) {
      registrationEndDate = "Closed";
    } else {
      registrationEndDate = regEndDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      });
    }
  } else if (startDate) {
    const startDateObj = new Date(startDate);
    startDateObj.setHours(0, 0, 0, 0);
    
    // Use start date as fallback, but check if it has passed
    if (startDateObj < today) {
      registrationEndDate = "Closed";
    } else {
      registrationEndDate = startDateObj.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      });
    }
  } else {
    registrationEndDate = "TBD";
  }

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
        label="Tournament Type"
        value={tournamentType || "N/A"}
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
