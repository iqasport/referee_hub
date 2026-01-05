import React from "react";
import { Link } from "react-router-dom";
import { useGetTournamentQuery } from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";
import {
  ChevronLeftIcon,
  LocationIcon,
  CalendarIcon,
  UsersIcon,
  HomeIcon,
  ClockIcon,
} from "../../../components/icons";

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();

  const { data: tournament, isLoading, isError } = useGetTournamentQuery({ tournamentId: tournamentId || "" });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p>Loading tournament...</p>
      </div>
    );
  }

  if (isError || !tournament) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p>Tournament not found</p>
      </div>
    );
  }

  const startDate = new Date(tournament.startDate || "");
  const endDate = new Date(tournament.endDate || "");
  const formattedDateRange = `${startDate.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  })} - ${endDate.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  })}`;

  return (
    <>
      {/* Header with back button overlay */}
      <header className="relative">
        <div className="relative h-48 overflow-hidden">
          <img
            src={tournament.bannerImageUrl || "https://placehold.co/1200x200"}
            alt="Tournament banner"
            className="w-full h-full object-cover"
          />
          {/* Back button overlay */}
          <Link to="/tournaments">
            <ChevronLeftIcon />
          </Link>
        </div>

        {/* Title overlay on image */}
        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black via-black/50 to-transparent p-8 text-white">
          <h1 className="text-4xl font-bold">{tournament.name}</h1>
          <div className="flex items-center gap-2 mt-2 text-sm opacity-90">
            <LocationIcon className="w-4 h-4" />
            <span>{tournament.country}</span>
          </div>
        </div>
      </header>

      {/* Info cards section */}
      <section className="bg-white px-6 py-8 -mt-12 relative z-10">
        <div className="max-w-6xl mx-auto">
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
              <p className="text-sm font-semibold text-gray-900">{tournament.organizer || "N/A"}</p>
            </div>

            {/* Registration Ends Card */}
            <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
              <div className="flex items-center gap-2 mb-2">
                <ClockIcon className="w-5 h-5 text-green-600" />
                <p className="text-xs text-gray-600 font-semibold">Registration Ends</p>
              </div>
              <p className="text-sm font-semibold text-gray-900">
                {new Date(tournament.startDate).toLocaleDateString("en-US", {
                  month: "short",
                  day: "numeric",
                  year: "numeric",
                })}
              </p>
            </div>
          </div>

          {/* Main content grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left column - About and Format */}
            <div className="lg:col-span-2">
              {/* About this tournament */}
              <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
                <h2 className="text-xl font-bold text-gray-900 mb-4">About This Tournament</h2>
                <p className="text-gray-700 leading-relaxed mb-4">{tournament.description}</p>
              </div>

              {/* Tournament Format (placeholder based on design) */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h3 className="text-lg font-bold text-gray-900 mb-3">Tournament Format</h3>
                <p className="text-gray-700 leading-relaxed">
                  Single elimination bracket with all teams. All games will be played according to
                  official rules. Each game consists of four 10-minute quarters with a 15-minute
                  halftime break.
                </p>
              </div>
            </div>

            {/* Right sidebar - Registration and Contact */}
            <div>
              {/* Register Now Card */}
              <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6 sticky top-4">
                <h3 className="text-lg font-bold text-gray-900 mb-2">Register Now</h3>
                <p className="text-sm text-gray-600 mb-4">
                  Secure your spot in this exciting tournament. Limited slots available!
                </p>
                <button className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors">
                  Register for Tournament
                </button>
              </div>

              {/* Need Help Card */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h3 className="text-lg font-bold text-gray-900 mb-2">Need Help?</h3>
                <p className="text-sm text-gray-600 mb-4">
                  Have questions about this tournament? Contact the organizers.
                </p>
                <button className="w-full bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded-lg transition-colors">
                  Contact Organizer
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
};

export default TournamentDetails;
