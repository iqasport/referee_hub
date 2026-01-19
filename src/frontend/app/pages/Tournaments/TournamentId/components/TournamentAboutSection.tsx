import React from "react";
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
    <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
      <h2 className="text-xl font-bold text-gray-900 mb-4">About This Tournament</h2>
      {/* Location */}
      <div className="flex items-start gap-2 mt-4">
        <LocationIcon className="w-5 h-5 text-green-600 mt-0.5" />
        <div>
          <p className="text-sm text-gray-700">{location}</p>
        </div>
      </div>
      <h2 className="text-xl font-bold text-gray-900 mb-4">Description</h2>
      <p className="text-gray-700 leading-relaxed mb-4">{description}</p>
    </div>
  );
};

export default TournamentAboutSection;
