import React from "react";
import { CalendarIcon, UsersIcon, HomeIcon, ClockIcon } from "../../../../components/icons";

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
  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      {/* Tournament Date Card */}
      <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
        <div className="flex items-center gap-2 mb-2">
          <CalendarIcon className="w-5 h-5 text-green-600" />
          <p className="text-xs text-gray-600 font-semibold">Tournament Date</p>
        </div>
        <p className="text-sm font-semibold text-gray-900">{formattedDateRange}</p>
      </div>

      {/* Participants Card */}
      <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
        <div className="flex items-center gap-2 mb-2">
          <UsersIcon className="w-5 h-5 text-green-600" />
          <p className="text-xs text-gray-600 font-semibold">Participants</p>
        </div>
        <p className="text-sm font-semibold text-gray-900">TBD</p>
      </div>

      {/* Organizer Card */}
      <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
        <div className="flex items-center gap-2 mb-2">
          <HomeIcon className="w-5 h-5 text-green-600" />
          <p className="text-xs text-gray-600 font-semibold">Organizer</p>
        </div>
        <p className="text-sm font-semibold text-gray-900">{organizer || "N/A"}</p>
      </div>

      {/* Registration Ends Card */}
      <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
        <div className="flex items-center gap-2 mb-2">
          <ClockIcon className="w-5 h-5 text-green-600" />
          <p className="text-xs text-gray-600 font-semibold">Registration Ends</p>
        </div>
        <p className="text-sm font-semibold text-gray-900">
          {startDate
            ? new Date(startDate).toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
                year: "numeric",
              })
            : "TBD"}
        </p>
      </div>
    </div>
  );
};

export default TournamentInfoCards;
