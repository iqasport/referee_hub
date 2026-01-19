import React from "react";

interface TournamentHeaderProps {
  bannerImageUrl?: string | null;
  name?: string | null;
  isManager?: boolean;
}

const TournamentHeader: React.FC<TournamentHeaderProps> = ({
  bannerImageUrl,
  name,
  isManager = false,
}) => {
  return (
    <header className="bg-white px-6">
      <div className="max-w-6xl mx-auto">
        <div
          className="relative h-48 overflow-hidden"
        >
          <img
            src={bannerImageUrl || "https://placehold.co/1200x200"}
            alt="Tournament banner"
            className="w-full h-full object-cover"
          />
          {/* Title overlay on image */}
          <div
            className="absolute bottom-0 left-0 right-0 p-8 text-white"
            style={{ background: "linear-gradient(to top, rgba(0,0,0,0.8), transparent)" }}
          >
            <div className="flex items-center justify-between">
              <h1 className="text-4xl font-bold">{name}</h1>
              {isManager && (
                <div className="bg-blue-600 px-3 py-1 rounded-full text-sm font-semibold">
                  Manager View
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default TournamentHeader;
