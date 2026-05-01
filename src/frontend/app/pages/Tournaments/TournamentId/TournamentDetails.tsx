import React, { useRef, useState } from "react";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ContactOrganizerModal, { ContactOrganizerModalRef } from "./ContactOrganizerModal";
import AddTournamentModal, { AddTournamentModalRef } from "../components/AddTournamentModal";
import RegistrationsModal, { RegistrationsModalRef } from "./RegistrationsModal";
import InviteTeamsModal, { InviteTeamsModalRef } from "./InviteTeamsModal";
import AddTournamentManagerModal from "./AddTournamentManagerModal";
import ActionButtonPair from "../../../components/ActionButtonPair";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import {
  TournamentHeader,
  TournamentInfoCards,
  TournamentAboutSection,
  RosterManager,
} from "./components";
import {
  useGetTournamentQuery,
  useGetPublicTournamentQuery,
  useGetTournamentManagersQuery,
  useGetCurrentUserQuery,
  useGetTournamentInvitesQuery,
  useRespondToInviteMutation,
  useGetManagedTeamsQuery,
  useGetParticipantsQuery,
  useDeleteTournamentMutation,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams, useNavigate } from "../../../utils/navigationUtils";
import {
  useTournamentPermissions,
  useApprovedTeams,
  usePendingInvites,
  useIsRegistrationClosed,
  useManagedTeamIds,
  useRosterStats,
} from "./hooks";

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef = useRef<ContactOrganizerModalRef>(null);
  const editModalRef = useRef<AddTournamentModalRef>(null);
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);
  const rosterSectionRef = useRef<HTMLDivElement>(null);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [isAddManagerModalOpen, setIsAddManagerModalOpen] = useState(false);
  const { alertState, showAlert, hideAlert } = useAlert();

  // Query for current user
  const {
    data: currentUser,
    isLoading: isCurrentUserLoading,
    isError: isCurrentUserError,
  } = useGetCurrentUserQuery();

  // Query for authenticated tournament view
  const shouldUseAuthenticatedTournamentQuery = !isCurrentUserLoading && !(isCurrentUserError || !currentUser);
  const {
    data: authenticatedTournament,
    isLoading: isLoadingAuthenticatedTournament,
    isError: isAuthenticatedTournamentError,
  } = useGetTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || !shouldUseAuthenticatedTournamentQuery }
  );

  // Query for public tournament view (anonymous users)
  const {
    data: publicTournament,
    isLoading: isLoadingPublicTournament,
    isError: isPublicTournamentError,
  } = useGetPublicTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || shouldUseAuthenticatedTournamentQuery }
  );

  // Determine which tournament data to use
  const isAnonymous = !isCurrentUserLoading && (isCurrentUserError || !currentUser);
  const tournament = isAnonymous
    ? (publicTournament ? { ...publicTournament, isCurrentUserInvolved: false } : undefined)
    : authenticatedTournament;
  const isLoading = isCurrentUserLoading || isLoadingAuthenticatedTournament || isLoadingPublicTournament;
  const isError = isAnonymous ? isPublicTournamentError : isAuthenticatedTournamentError;

  // Query managed teams
  const { data: managedTeamsData } = useGetManagedTeamsQuery(undefined, {
    skip: isAnonymous,
  });

  // Check if user is a tournament manager for this specific tournament
  const isTournamentManagerOfThis = currentUser?.roles?.some((role: any) => {
    if (role.roleType !== "TournamentManager") return false;
    if (role.tournament === "ANY") return true;
    if (Array.isArray(role.tournament)) {
      return role.tournament.includes(tournamentId);
    }
    return role.tournament === tournamentId;
  });

  // Query tournament managers (only if user is a manager of this tournament)
  const shouldFetchManagers = Boolean(tournamentId && isTournamentManagerOfThis);
  const { data: managers, isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

  // Query tournament invites
  const { data: invites, refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Query participants for roster information
  const { data: participants, refetch: refetchParticipants } = useGetParticipantsQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Mutations
  const [respondToInvite] = useRespondToInviteMutation();
  const [deleteTournament] = useDeleteTournamentMutation();
  const navigate = useNavigate();

  // Extract permissions state using custom hook
  const { isTournamentManager } = useTournamentPermissions(
    currentUser,
    managers,
    managersError
  );

  // Extract managed team IDs
  const managedTeamIds = useManagedTeamIds(managedTeamsData);

  // Extract pending and approved invites
  const pendingInvitesForUser = usePendingInvites(invites, managedTeamIds);
  const approvedTeamsForUser = useApprovedTeams(invites, managedTeamIds, managedTeamsData);

  // Extract roster stats
  const { totalPlayerCount } = useRosterStats(participants);

  // Check if registration is closed
  const isRegistrationClosed = useIsRegistrationClosed(tournament);

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

      showAlert(approved ? "Successfully accepted the invite!" : "Invite declined.", "success");
      refetchInvites();
    } catch (error) {
      console.error("Failed to respond to invite:", error);
      showAlert("Failed to respond. Please try again.", "error");
    } finally {
      setRespondingTo(null);
    }
  }

  async function handleDelete() {
    if (!tournamentId) return;
    if (!window.confirm(`Are you sure you want to delete "${tournament?.name ?? "this tournament"}"? It will be removed from view.`)) return;
    try {
      await deleteTournament({ tournamentId }).unwrap();
      navigate("/tournaments");
    } catch (error) {
      console.error("Failed to delete tournament:", error);
      showAlert("Failed to delete the tournament. Please try again.", "error");
    }
  }

  if (isLoading) {
    return (
      <div className="tournament-details-loading">
        <p>Loading tournament...</p>
      </div>
    );
  }

  if (isError || !tournament) {
    return (
      <div className="tournament-details-error">
        <p>Tournament not found</p>
      </div>
    );
  }

  const startDate = new Date(tournament.startDate || "");
  const endDate = new Date(tournament.endDate || "");

  // Check if start and end dates are the same
  const isSameDay = startDate.toDateString() === endDate.toDateString();

  const formattedDateRange = isSameDay
    ? startDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      })
    : `${startDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      })} - ${endDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      })}`;

  // Handle edit tournament (for managers)
  const handleEdit = () => {
    editModalRef.current?.openEdit({
      id: tournament.id || "",
      name: tournament.name || "",
      description: tournament.description || "",
      startDate: tournament.startDate || "",
      endDate: tournament.endDate || "",
      registrationEndsDate: tournament.registrationEndsDate || "",
      type: tournament.type || ("" as const),
      country: tournament.country || "",
      city: tournament.city || "",
      place: tournament.place || "",
      organizer: tournament.organizer || "",
      isPrivate: tournament.isPrivate || false,
      isRegistrationOpen: tournament.isRegistrationOpen ?? true,
      bannerImageUrl: tournament.bannerImageUrl || "",
    });
  };

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}

      <TournamentHeader
        bannerImageUrl={tournament.bannerImageUrl}
        name={tournament.name}
        isManager={isTournamentManager}
      />

      {/* Info cards section */}
      <section className="tournament-details-section">
        <div className="tournament-details-wrapper">
          <TournamentInfoCards
            formattedDateRange={formattedDateRange}
            organizer={tournament.organizer}
            startDate={tournament.startDate}
            registrationEndsDate={tournament.registrationEndsDate}
            isRegistrationOpen={tournament.isRegistrationOpen}
            tournamentType={tournament.type}
          />

          {/* Main content grid */}
          <div className="tournament-details-grid">
            {/* Left column - About and Format */}
            <div>
              <TournamentAboutSection
                place={tournament.place}
                city={tournament.city}
                country={tournament.country}
                description={tournament.description}
              />
            </div>

            {/* Right sidebar - Different content for managers vs regular users */}
            <div>
              {isTournamentManager ? (
                <>
                  {/* Manager Tools Card */}
                  <div className="card card-highlighted card-mb card-sticky">
                    <h3 className="card-title">Manager Tools</h3>
                    <p className="card-description">
                      You are the manager of this tournament. Use the tools below to manage the
                      tournament.
                    </p>
                    <button
                      onClick={handleEdit}
                      className="btn btn-primary btn-full-width btn-with-icon card-mb"
                    >
                      <svg
                        className="btn-icon"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
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
                      onClick={() => registrationsModalRef.current?.open(tournament.id || "", tournament.name || "Unknown Tournament")}
                      className="btn btn-secondary btn-full-width card-mb"
                    >
                      View Team Registrations ({invites?.length || 0})
                    </button>
                    <button
                      onClick={() => inviteTeamsModalRef.current?.open(tournament)}
                      className="btn btn-secondary btn-full-width card-mb"
                    >
                      Invite Teams
                    </button>
                    <button
                      onClick={() => setIsAddManagerModalOpen(true)}
                      className="btn btn-secondary btn-full-width"
                    >
                      Add Tournament Manager
                    </button>
                    <button
                      onClick={handleDelete}
                      className="btn btn-danger btn-full-width"
                      style={{ marginTop: "0.75rem" }}
                    >
                      Delete Tournament
                    </button>
                  </div>

                  {/* Tournament Stats Card */}
                  <div className="card">
                    <h3 className="card-title">Tournament Stats</h3>
                    <div className="stats-list">
                      <div className="stats-item">
                        <span className="stats-label">Teams Registered</span>
                        <span className="stats-value">
                          {invites?.filter((i) => i.status === "approved").length || 0}
                        </span>
                      </div>
                      <div className="stats-item">
                        <span className="stats-label">Players Registered</span>
                        <span className="stats-value">{totalPlayerCount}</span>
                      </div>
                      <div className="stats-item">
                        <span className="stats-label">Private Tournament</span>
                        <span className="stats-value">{tournament.isPrivate ? "Yes" : "No"}</span>
                      </div>
                    </div>
                  </div>
                </>
              ) : isAnonymous ? (
                <div className="card card-highlighted card-mb card-sticky">
                  <h3 className="card-title">Sign In To Participate</h3>
                  <p className="card-description">
                    This tournament is visible publicly. Sign in to register teams, manage rosters,
                    or contact organizers.
                  </p>
                  <a className="btn btn-primary btn-full-width" href="/sign_in">
                    Sign In
                  </a>
                </div>
              ) : (
                <>
                  {pendingInvitesForUser.length > 0 && (
                    <div className="card card-highlighted card-mb">
                      <h3 className="card-title">You&apos;re Invited!</h3>
                      <p className="card-description">
                        The tournament organizer has invited your team(s) to participate.
                      </p>
                      <div className="invite-list">
                        {pendingInvitesForUser.map((invite) => (
                          <div key={invite.participantId} className="invite-item">
                            <p className="invite-team-name">{invite.participantName}</p>
                            <ActionButtonPair
                              onAccept={() =>
                                handleRespondToInvite(invite.participantId || "", true)
                              }
                              onDecline={() =>
                                handleRespondToInvite(invite.participantId || "", false)
                              }
                              isLoading={respondingTo === invite.participantId}
                              size="sm"
                            />
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Register Now / Manage Rosters Card */}
                  <div className="card card-mb">
                    {isRegistrationClosed ? (
                      <>
                        <h3 className="card-title">Registration Closed</h3>
                        <p className="card-description">
                          Registration for this tournament is now closed. Contact the organizers if you have questions.
                        </p>
                        <div className="mt-4 p-3 bg-gray-100 rounded-md border border-gray-300 text-center">
                          <svg className="mx-auto h-12 w-12 text-gray-400 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                          </svg>
                          <p className="text-sm font-medium text-gray-700">Registration is now closed</p>
                        </div>
                      </>
                    ) : approvedTeamsForUser.length > 0 ? (
                      <>
                        <h3 className="card-title">You&apos;re Registered!</h3>
                        <p className="card-description">
                          Your team is registered for this tournament. Manage your roster below.
                        </p>
                        <button
                          onClick={() =>
                            rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })
                          }
                          className="btn btn-primary btn-full-width card-mb"
                        >
                          Manage Your Rosters
                        </button>
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
                          className="btn btn-outline btn-full-width"
                        >
                          Register Another Team
                        </button>
                      </>
                    ) : (
                      <>
                        <h3 className="card-title">Register Now</h3>
                        <p className="card-description">
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
                          className="btn btn-primary btn-full-width"
                        >
                          Register for Tournament
                        </button>
                      </>
                    )}
                  </div>

                  {/* Need Help Card */}
                  <div className="card">
                    <h3 className="card-title">Need Help?</h3>
                    <p className="card-description">
                      Have questions about this tournament? Contact the organizers.
                    </p>
                    <button
                      onClick={() =>
                        contactOrganizerModalRef.current?.open({
                          name: tournament.organizer || "",
                          tournamentName: tournament.name || "",
                          tournamentId: tournament.id,
                        })
                      }
                      className="btn btn-outline btn-full-width"
                    >
                      Contact Organizer
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Roster Management Section - Show for team managers with approved teams */}
          {approvedTeamsForUser.length > 0 && (
            <div ref={rosterSectionRef} className="roster-section">
              <h2 className="card-title card-title-lg">Manage Your Team Rosters</h2>
              <RosterManager
                tournamentId={tournamentId || ""}
                teams={approvedTeamsForUser}
                onRosterSaved={() => {
                  refetchInvites();
                  refetchParticipants();
                }}
              />
            </div>
          )}
        </div>
      </section>

      {/* Regular user modals */}
      {!isAnonymous && <RegisterTournamentModal ref={registerModalRef} />}
      {!isAnonymous && <ContactOrganizerModal ref={contactOrganizerModalRef} />}

      {/* Manager modals */}
      {!isAnonymous && <AddTournamentModal ref={editModalRef} />}
      {!isAnonymous && <RegistrationsModal ref={registrationsModalRef} />}
      {!isAnonymous && <InviteTeamsModal ref={inviteTeamsModalRef} />}
      {!isAnonymous && isAddManagerModalOpen && tournamentId && (
        <AddTournamentManagerModal
          tournamentId={tournamentId}
          onClose={() => setIsAddManagerModalOpen(false)}
        />
      )}
    </>
  );
};

export default TournamentDetails;
