import React, { useRef, useMemo, useState } from "react";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ContactOrganizerModal, { ContactOrganizerModalRef } from "./ContactOrganizerModal";
import ManagerView from "./ManagerView";
import {
  TournamentHeader,
  TournamentNavBar,
  TournamentInfoCards,
  TournamentAboutSection,
  TournamentFormatSection,
} from "./components";
import {
  useGetTournamentQuery,
  useGetTournamentManagersQuery,
  useGetCurrentUserQuery,
  useGetTournamentInvitesQuery,
  useRespondToInviteMutation,
  useGetManagedTeamsQuery,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef = useRef<ContactOrganizerModalRef>(null);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);

  const {
    data: tournament,
    isLoading,
    isError,
  } = useGetTournamentQuery({ tournamentId: tournamentId || "" });
  const { data: currentUser } = useGetCurrentUserQuery();

  // Use the new managed teams endpoint to get teams the user manages
  const { data: managedTeamsData } = useGetManagedTeamsQuery();

  // Check if user is a tournament manager - only tournament managers can view the managers list
  const isTournamentManager = currentUser?.roles?.some(
    (role: any) => role.roleType === "TournamentManager"
  );

  // Only fetch managers if user is a tournament manager
  const shouldFetchManagers = Boolean(tournamentId && isTournamentManager);
  const { data: managers, isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

  // Fetch tournament invites to check for pending invites for user's teams
  const { data: invites, refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId }
  );

  const [respondToInvite] = useRespondToInviteMutation();

  // Get team IDs from the managed teams endpoint
  const managedTeamIds: Set<string> = useMemo(() => {
    const teamIds = new Set<string>();
    if (managedTeamsData) {
      managedTeamsData.forEach((team) => {
        if (team.teamId) {
          teamIds.add(team.teamId);
        }
      });
    }
    return teamIds;
  }, [managedTeamsData]);

  // Find pending invites for user's managed teams (where participantApproval is pending)
  // These are invites initiated by tournament managers that the team manager needs to accept/decline
  const pendingInvitesForUser: TournamentInviteViewModel[] = useMemo(() => {
    if (!invites || managedTeamIds.size === 0) return [];

    return invites.filter((invite) => {
      // Check if this invite is for one of user's teams
      if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;

      // Check if participant approval is pending (user needs to respond)
      return invite.participantApproval?.status === "pending";
    });
  }, [invites, managedTeamIds]);

  // Handle accept/decline invite
  async function handleRespondToInvite(participantId: string, approved: boolean) {
    if (!tournamentId) return;

    setRespondingTo(participantId);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved },
      }).unwrap();

      alert(approved ? "Successfully accepted the invite!" : "Invite declined.");
      refetchInvites();
    } catch (error) {
      console.error("Failed to respond to invite:", error);
      alert("Failed to respond. Please try again.");
    } finally {
      setRespondingTo(null);
    }
  }

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
  const isManager =
    !managersError && currentUser?.userId && managers
      ? managers.some((manager) => manager.id === currentUser.userId)
      : false;

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
      <TournamentHeader bannerImageUrl={tournament.bannerImageUrl} name={tournament.name} />

      <TournamentNavBar />

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

            {/* Right sidebar - Registration and Contact */}
            <div>
              {/* Pending Invites Card - Show when user has pending invites to respond to */}
              {pendingInvitesForUser.length > 0 && (
                <div className="bg-yellow-50 rounded-lg border border-yellow-200 p-6 mb-6">
                  <h3 className="text-lg font-bold text-gray-900 mb-2">You&apos;re Invited!</h3>
                  <p className="text-sm text-gray-600 mb-4">
                    The tournament organizer has invited your team(s) to participate.
                  </p>
                  <div className="space-y-3">
                    {pendingInvitesForUser.map((invite) => (
                      <div
                        key={invite.participantId}
                        className="bg-white rounded-lg border border-gray-200 p-4"
                      >
                        <p className="font-semibold text-gray-900 mb-3">{invite.participantName}</p>
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleRespondToInvite(invite.participantId || "", true)}
                            disabled={respondingTo === invite.participantId}
                            className="px-4 py-2 text-sm font-medium rounded-lg"
                            style={{
                              flex: 1,
                              backgroundColor: "#16a34a",
                              color: "white",
                              opacity: respondingTo === invite.participantId ? 0.5 : 1,
                              cursor:
                                respondingTo === invite.participantId ? "not-allowed" : "pointer",
                            }}
                          >
                            {respondingTo === invite.participantId ? "..." : "Accept"}
                          </button>
                          <button
                            onClick={() => handleRespondToInvite(invite.participantId || "", false)}
                            disabled={respondingTo === invite.participantId}
                            className="flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-3 rounded-lg transition-colors text-sm"
                            style={{
                              opacity: respondingTo === invite.participantId ? 0.5 : 1,
                              cursor:
                                respondingTo === invite.participantId ? "not-allowed" : "pointer",
                            }}
                          >
                            {respondingTo === invite.participantId ? "..." : "Decline"}
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Register Now Card */}
              <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6 sticky top-4">
                <h3 className="text-lg font-bold text-gray-900 mb-2">Register Now</h3>
                <p className="text-sm text-gray-600 mb-4">
                  Secure your spot in this exciting tournament. Limited slots available!
                </p>
                <button
                  onClick={() =>
                    registerModalRef.current?.open({
                      id: tournament.id || "",
                      name: tournament.name || "",
                      startDate: tournament.startDate || "",
                      endDate: tournament.endDate || "",
                      country: tournament.country || "",
                      city: tournament.city || "",
                      type: tournament.type || "",
                    })
                  }
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
                <button
                  onClick={() =>
                    contactOrganizerModalRef.current?.open({
                      name: tournament.organizer || "",
                      tournamentName: tournament.name || "",
                    })
                  }
                  className="w-full bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded-lg transition-colors"
                >
                  Contact Organizer
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      <RegisterTournamentModal ref={registerModalRef} />
      <ContactOrganizerModal ref={contactOrganizerModalRef} />
    </>
  );
};

export default TournamentDetails;
