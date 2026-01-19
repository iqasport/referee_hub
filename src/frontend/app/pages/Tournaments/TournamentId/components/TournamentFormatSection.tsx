import React from "react";

interface TournamentFormatSectionProps {
  format?: string;
}

const TournamentFormatSection: React.FC<TournamentFormatSectionProps> = ({ format }) => {
  const defaultFormat =
    "Single elimination bracket with all teams. All games will be played according to official rules. Each game consists of four 10-minute quarters with a 15-minute halftime break.";

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6">
      <h3 className="text-lg font-bold text-gray-900 mb-3">Tournament Format</h3>
      <p className="text-gray-700 leading-relaxed">{format || defaultFormat}</p>
    </div>
  );
};

export default TournamentFormatSection;
