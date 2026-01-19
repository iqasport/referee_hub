import React from "react";
import { CalendarIcon, UsersIcon, HomeIcon, ClockIcon } from "../../../../components/icons";
import InfoCard from "../../../../components/InfoCard";

interface TournamentInfoCardsProps {
  formattedDateRange: string;
  organizer?: string | null;
  startDate?: string;
}

const TournamentInfoCards: React.FC<TournamentInfoCardsProps> = ({
  formattedDateRange,
  organizer,
  startDate,
}) => {
  const registrationEndDate = startDate
    ? new Date(startDate).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      })
    : "TBD";

  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <InfoCard
        icon={<CalendarIcon className="w-5 h-5" />}
        label="Tournament Date"
        value={formattedDateRange}
      />
      <InfoCard icon={<UsersIcon className="w-5 h-5" />} label="Participants" value="TBD" />
      <InfoCard
        icon={<HomeIcon className="w-5 h-5" />}
        label="Organizer"
        value={organizer || "N/A"}
      />
      <InfoCard
        icon={<ClockIcon className="w-5 h-5" />}
        label="Registration Ends"
        value={registrationEndDate}
      />
    </div>
  );
};

export default TournamentInfoCards;
