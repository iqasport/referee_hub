import React, { useRef } from "react";
import { Link } from "react-router-dom";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ManagerView from "./ManagerView";
import { useGetTournamentQuery, useGetTournamentManagersQuery, useGetCurrentUserQuery } from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";
import {
  CalendarIcon,
  UsersIcon,
  HomeIcon,
  ClockIcon,
} from "../../../components/icons";

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);

  const { data: tournament, isLoading, isError } = useGetTournamentQuery({ tournamentId: tournamentId || "" });
  const { data: currentUser } = useGetCurrentUserQuery();
  
  // Check if user is a tournament manager - only tournament managers can view the managers list
  const isTournamentManager = currentUser?.roles?.some((role: any) => role.roleType === "TournamentManager");
  
  // Only fetch managers if user is a tournament manager
  const shouldFetchManagers = Boolean(tournamentId && isTournamentManager);
  const { data: managers, isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

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

  // Check if current user is a manager of this tournament
  // Only consider them a manager if they're in the managers list and we successfully fetched the list
  const isManager = !managersError && currentUser?.userId && managers ? managers.some((manager) => manager.id === currentUser.userId) : false;

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

  // Show manager view if user is a manager
  if (isManager) {
    return <ManagerView tournament={tournament} />;
  }

  return (
    <>
      {/* Header */}
      <header>
        <div className="relative h-48 overflow-hidden">
          <img
            src={tournament.bannerImageUrl || "https://placehold.co/1200x200"}
            alt="Tournament banner"
            className="w-full h-full object-cover"
          />
          {/* Title overlay on image */}
          <div className="absolute bottom-0 left-0 right-0 p-8 text-white" style={{ background: 'linear-gradient(to top, rgba(0,0,0,0.8), transparent)' }}>
            <h1 className="text-4xl font-bold">{tournament.name}</h1>
          </div>
        </div>
      </header>

      {/* Back button */}
      <div className="bg-white px-6 py-3 border-b border-gray-200">
        <Link
          to="/tournaments"
          className="text-blue-600 font-medium"
        >
          ← Back to Tournaments
        </Link>
      </div>

      {/* Info cards section */}
      <section className="bg-white px-6 py-8">
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
                <button
                  onClick={() => registerModalRef.current?.open({
                    id: tournament.id || "",
                    name: tournament.name || "",
                    startDate: tournament.startDate || "",
                    endDate: tournament.endDate || "",
                    country: tournament.country || "",
                    city: tournament.city || "",
                    type: tournament.type || "",
                  })}
                  className="w-full bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded-lg transition-colors"
                >
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

      <RegisterTournamentModal ref={registerModalRef} />
    </>
  );
};

export default TournamentDetails;
