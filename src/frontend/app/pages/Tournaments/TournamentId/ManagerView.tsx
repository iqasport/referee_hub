import React, { useRef } from "react";
import { TournamentViewModel, useGetTournamentInvitesQuery } from "../../../store/serviceApi";
import AddTournamentModal, { AddTournamentModalRef } from "../components/AddTournamentModal";
import RegistrationsModal, { RegistrationsModalRef } from "./RegistrationsModal";
import InviteTeamsModal, { InviteTeamsModalRef } from "./InviteTeamsModal";
import {
  TournamentHeader,
  TournamentNavBar,
  TournamentInfoCards,
  TournamentAboutSection,
  TournamentFormatSection,
} from "./components";

interface ManagerViewProps {
  tournament: TournamentViewModel;
}

const ManagerView: React.FC<ManagerViewProps> = ({ tournament }) => {
  const editModalRef = useRef<AddTournamentModalRef>(null);
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);

  // Fetch tournament invites
  const { data: invites } = useGetTournamentInvitesQuery({
    tournamentId: tournament.id || "",
  });

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

  const handleEdit = () => {
    editModalRef.current?.openEdit({
      id: tournament.id || "",
      name: tournament.name || "",
      description: tournament.description || "",
      startDate: tournament.startDate || "",
      endDate: tournament.endDate || "",
      type: tournament.type || ("" as const),
      country: tournament.country || "",
      city: tournament.city || "",
      place: tournament.place || "",
      organizer: tournament.organizer || "",
      isPrivate: tournament.isPrivate || false,
      bannerImageUrl: tournament.bannerImageUrl || "",
    });
  };

  return (
    <>
      <TournamentHeader
        bannerImageUrl={tournament.bannerImageUrl}
        name={tournament.name}
        isManager
      />

      <TournamentNavBar isManager onEdit={handleEdit} />

      {/* Info cards section */}
      <section className="bg-white px-6 py-8">
        <div className="max-w-6xl mx-auto">
          <TournamentInfoCards
            formattedDateRange={formattedDateRange}
            organizer={tournament.organizer}
            startDate={tournament.startDate}
          />

          {/* Main content grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left column - About and Format */}
            <div className="lg:col-span-2">
              <TournamentAboutSection
                place={tournament.place}
                city={tournament.city}
                country={tournament.country}
                description={tournament.description}
              />

              <TournamentFormatSection />
            </div>

            {/* Right sidebar - Manager Tools */}
            <div>
              {/* Manager Tools Card */}
              <div className="bg-blue-50 rounded-lg border border-blue-200 p-6 mb-6 sticky top-4">
                <h3 className="text-lg font-bold text-gray-900 mb-2">Manager Tools</h3>
                <p className="text-sm text-gray-600 mb-4">
                  You are the manager of this tournament. Use the tools below to manage the
                  tournament.
                </p>
                <button
                  onClick={handleEdit}
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors mb-3 flex items-center justify-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                    />
                  </svg>
                  Edit Tournament Details
                </button>
                <button
                  onClick={() => registrationsModalRef.current?.open(tournament.id || "")}
                  className="w-full bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded-lg transition-colors mb-3"
                >
                  View Team Registrations ({invites?.length || 0})
                </button>
                <button
                  onClick={() => inviteTeamsModalRef.current?.open(tournament)}
                  className="w-full bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded-lg transition-colors mb-3"
                >
                  Invite Teams
                </button>
              </div>

              {/* Tournament Stats Card */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h3 className="text-lg font-bold text-gray-900 mb-4">Tournament Stats</h3>
                <div className="space-y-3">
                  <div className="flex justify-between items-center pb-3 border-b border-gray-200">
                    <span className="text-sm text-gray-600">Teams Registered</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {invites?.filter((i) => i.status === "approved").length || 0}
                    </span>
                  </div>
                  <div className="flex justify-between items-center pb-3 border-b border-gray-200">
                    <span className="text-sm text-gray-600">Private Tournament</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {tournament.isPrivate ? "Yes" : "No"}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-gray-600">Tournament Type</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {tournament.type || "N/A"}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <AddTournamentModal ref={editModalRef} />
      <RegistrationsModal ref={registrationsModalRef} />
      <InviteTeamsModal ref={inviteTeamsModalRef} />
    </>
  );
};

export default ManagerView;
