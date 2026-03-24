import React, { useRef, useMemo, useState } from "react";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ContactOrganizerModal, { ContactOrganizerModalRef } from "./ContactOrganizerModal";
import AddTournamentModal, { AddTournamentModalRef } from "../components/AddTournamentModal";
import RegistrationsModal, { RegistrationsModalRef } from "./RegistrationsModal";
import InviteTeamsModal, { InviteTeamsModalRef } from "./InviteTeamsModal";
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
  useGetTournamentManagersQuery,
  useGetCurrentUserQuery,
  useGetTournamentInvitesQuery,
  useRespondToInviteMutation,
  useGetManagedTeamsQuery,
  useGetParticipantsQuery,
  useAddTournamentManagerMutation,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";
import { getApiErrorMessage } from "../../../utils/tournamentUtils";

function formatDateRange(startDateStr?: string | null, endDateStr?: string | null): string {
  const startDate = new Date(startDateStr || "");
  const endDate = new Date(endDateStr || "");
  const isSameDay = startDate.toDateString() === endDate.toDateString();
  return isSameDay
    ? startDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })
    : `${startDate.toLocaleDateString("en-US", { month: "short", day: "numeric" })} - ${endDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}`;
}

/** Extended role shape returned by the API (tournament field is not in the generated type). */
interface UserRole {
  roleType?: string;
  tournament?: string | string[];
}

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef = useRef<ContactOrganizerModalRef>(null);
  const editModalRef = useRef<AddTournamentModalRef>(null);
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);
  const rosterSectionRef = useRef<HTMLDivElement>(null);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [addManagerEmail, setAddManagerEmail] = useState("");
  const [isAddManagerOpen, setIsAddManagerOpen] = useState(false);
  const { alertState, showAlert, hideAlert } = useAlert();

  const {
    data: tournament,
    isLoading,
    isError,
  } = useGetTournamentQuery({ tournamentId: tournamentId || "" });
  const { data: currentUser } = useGetCurrentUserQuery();

  // Use the new managed teams endpoint to get teams the user manages
  const { data: managedTeamsData } = useGetManagedTeamsQuery(undefined, { skip: !currentUser });

  // Check if user is a tournament manager for this specific tournament
  // Note: role.tournament can be "ANY", a single tournament ID string, or an array of tournament IDs
  const isTournamentManagerOfThis = useMemo(
    () => (currentUser?.roles as UserRole[] | undefined)?.some((role) => {
      if (role.roleType !== "TournamentManager") return false;
      if (role.tournament === "ANY") return true;
      if (Array.isArray(role.tournament)) return role.tournament.includes(tournamentId ?? "");
      return role.tournament === tournamentId;
    }),
    [currentUser?.roles, tournamentId],
  );

  // Only fetch managers if user is a tournament manager of this tournament
  const shouldFetchManagers = Boolean(tournamentId && isTournamentManagerOfThis);
  const { data: managers, isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

  // Fetch tournament invites to check for pending invites for user's teams
  const { data: invites, refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId }
  );

  // Fetch participants to get roster counts
  const { data: participants, refetch: refetchParticipants } = useGetParticipantsQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId }
  );

  const [respondToInvite] = useRespondToInviteMutation();
  const [addTournamentManager, { isLoading: isAddingManager }] = useAddTournamentManagerMutation();

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

  // These are invites initiated by tournament managers that the team manager needs to accept/decline
  const pendingInvitesForUser: TournamentInviteViewModel[] = useMemo(() => {
    if (!invites || managedTeamIds.size === 0) return [];
    return invites.filter((invite) => {
      if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;
      return invite.participantApproval?.status === "pending";
    });
  }, [invites, managedTeamIds]);

  // Teams that have been approved for this tournament
  const approvedTeamsForUser = useMemo(() => {
    if (!invites || managedTeamIds.size === 0 || !managedTeamsData) return [];
    return invites
      .filter((i) => i.participantId && managedTeamIds.has(i.participantId) && i.status === "approved")
      .map((i) => {
        const td = managedTeamsData.find((t) => t.teamId === i.participantId);
        return {
          teamId: i.participantId,
          teamName: i.participantName ?? td?.teamName ?? "Unknown Team",
          ngb: td?.ngb ?? "",
        };
      });
  }, [invites, managedTeamIds, managedTeamsData]);

  const totalPlayerCount = useMemo(
    () => participants?.reduce((sum, t) => sum + (t.players?.length ?? 0), 0) ?? 0,
    [participants],
  );

  const isRegistrationClosed = useMemo(() => {
    if (tournament?.isRegistrationOpen === false) return true;
    const ref = tournament?.registrationEndsDate ?? tournament?.startDate;
    if (!ref) return false;
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const d = new Date(ref); d.setHours(0, 0, 0, 0);
    return today > d;
  }, [tournament?.isRegistrationOpen, tournament?.registrationEndsDate, tournament?.startDate]);

  async function handleRespondToInvite(participantId: string, approved: boolean) {
    if (!tournamentId) return;
    setRespondingTo(participantId);
    try {
      await respondToInvite({ tournamentId, participantId, inviteResponseModel: { approved } }).unwrap();
      showAlert(approved ? "Successfully accepted the invite!" : "Invite declined.", "success");
      refetchInvites();
    } catch (error) {
      console.error("Failed to respond to invite:", error);
      showAlert(getApiErrorMessage(error, "Failed to respond to the invite. Please try again."), "error");
    } finally {
      setRespondingTo(null);
    }
  }

  async function handleAddManager(e: React.FormEvent) {
    e.preventDefault();
    if (!addManagerEmail.trim() || !tournamentId) return;
    try {
      await addTournamentManager({
        tournamentId,
        addTournamentManagerModel: { email: addManagerEmail.trim() },
      }).unwrap();
      showAlert("Successfully added manager.", "success");
      setAddManagerEmail("");
      setIsAddManagerOpen(false);
    } catch (error) {
      console.error("Failed to add manager:", error);
      showAlert(getApiErrorMessage(error, "Failed to add manager. Check that the email belongs to a registered user."), "error");
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

  const isManager =
    !managersError && currentUser?.userId && managers
      ? managers.some((manager) => manager.id === currentUser.userId)
      : false;

  const formattedDateRange = formatDateRange(tournament.startDate, tournament.endDate);

  const openRegisterModal = () => registerModalRef.current?.open({
    id: tournament.id || "",
    name: tournament.name || "",
    startDate: tournament.startDate || "",
    endDate: tournament.endDate || "",
    country: tournament.country || "",
    city: tournament.city || "",
    type: tournament.type || "",
  });

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}

      <TournamentHeader
        bannerImageUrl={tournament.bannerImageUrl}
        name={tournament.name}
        isManager={isManager}
      />

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

          <div className="tournament-details-grid">
            <div>
              <TournamentAboutSection
                place={tournament.place}
                city={tournament.city}
                country={tournament.country}
                description={tournament.description}
              />
            </div>

            <div>
              {isManager ? (
                <>
                  {/* Manager Tools Card */}
                  <div className="card card-highlighted card-mb card-sticky">
                    <h3 className="card-title">Manager Tools</h3>
                    <p className="card-description">
                      You are the manager of this tournament. Use the tools below to manage the tournament.
                    </p>
                    <button
                      onClick={() => editModalRef.current?.openEdit({
                        id: tournament.id || "",
                        name: tournament.name || "",
                        description: tournament.description || "",
                        startDate: tournament.startDate || "",
                        endDate: tournament.endDate || "",
                        registrationEndsDate: tournament.registrationEndsDate || "",
                        type: tournament.type as any,
                        country: tournament.country || "",
                        city: tournament.city || "",
                        place: tournament.place || "",
                        organizer: tournament.organizer || "",
                        isPrivate: tournament.isPrivate || false,
                        isRegistrationOpen: tournament.isRegistrationOpen ?? true,
                        bannerImageUrl: tournament.bannerImageUrl || "",
                      })}
                      className="btn btn-primary btn-full-width btn-with-icon card-mb"
                    >
                      <svg className="btn-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
                      onClick={() => setIsAddManagerOpen(!isAddManagerOpen)}
                      className="btn btn-secondary btn-full-width"
                    >
                      Add Tournament Manager
                    </button>
                  </div>

                  {/* Add Manager Form */}
                  {isAddManagerOpen && (
                    <div className="card card-mb">
                      <h3 className="card-title">Add Tournament Manager</h3>
                      <p className="card-description">Enter the email address of the user you want to add as a tournament manager.</p>
                      <form onSubmit={handleAddManager} className="space-y-3">
                        <input
                          type="email"
                          value={addManagerEmail}
                          onChange={(e) => setAddManagerEmail(e.target.value)}
                          placeholder="Email address"
                          required
                          className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                        />
                        <div className="flex gap-2">
                          <button type="submit" disabled={isAddingManager || !addManagerEmail.trim()} className="btn btn-primary">
                            {isAddingManager ? "Adding..." : "Add Manager"}
                          </button>
                          <button type="button" onClick={() => { setIsAddManagerOpen(false); setAddManagerEmail(""); }} className="btn btn-secondary">Cancel</button>
                        </div>
                      </form>
                    </div>
                  )}

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

                  {pendingInvitesForUser.length > 0 && (
                    <div className="card card-highlighted card-mb">
                      <h3 className="card-title">You&apos;re Invited!</h3>
                      <p className="card-description">
                        Your team(s) have been invited to participate in this tournament.
                      </p>
                      <div className="invite-list">
                        {pendingInvitesForUser.map((invite) => (
                          <div key={invite.participantId} className="invite-item">
                            <p className="invite-team-name">{invite.participantName}</p>
                            <ActionButtonPair
                              onAccept={() => handleRespondToInvite(invite.participantId || "", true)}
                              onDecline={() => handleRespondToInvite(invite.participantId || "", false)}
                              isLoading={respondingTo === invite.participantId}
                              size="sm"
                            />
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {(managedTeamsData?.length ?? 0) > 0 && (
                    isRegistrationClosed ? (
                      <div className="card card-mb">
                        {approvedTeamsForUser.length > 0 ? (
                          <>
                            <h3 className="card-title">Your Teams Are Registered</h3>
                            <p className="card-description">Your team(s) are registered. Manage your rosters below.</p>
                            <button onClick={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })} className="btn btn-primary btn-full-width">
                              Manage Your Rosters
                            </button>
                          </>
                        ) : (
                          <>
                            <h3 className="card-title">Registration Closed</h3>
                            <p className="card-description">Registration for this tournament is now closed.</p>
                          </>
                        )}
                      </div>
                    ) : approvedTeamsForUser.length > 0 ? (
                      <div className="card card-mb">
                        <h3 className="card-title">Your Teams Are Registered!</h3>
                        <p className="card-description">Your team(s) are registered. Manage your rosters or register another team.</p>
                        <button onClick={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })} className="btn btn-primary btn-full-width card-mb">
                          Manage Your Rosters
                        </button>
                        <button onClick={openRegisterModal} className="btn btn-outline btn-full-width">
                          Register Another Team
                        </button>
                      </div>
                    ) : (
                      <div className="card card-mb">
                        <h3 className="card-title">Register Your Team</h3>
                        <p className="card-description">You also manage teams. You can register them for this tournament.</p>
                        <button onClick={openRegisterModal} className="btn btn-outline btn-full-width">
                          Register a Team
                        </button>
                      </div>
                    )
                  )}
                </>
              ) : (
                <>
                  {/* Tournament Stats for users too */}
                  <div className="card card-mb">
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
                          onClick={openRegisterModal}
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
                          onClick={openRegisterModal}
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
      <RegisterTournamentModal ref={registerModalRef} />
      <ContactOrganizerModal ref={contactOrganizerModalRef} />

      {/* Manager modals */}
      <AddTournamentModal ref={editModalRef} />
      <RegistrationsModal ref={registrationsModalRef} />
      <InviteTeamsModal ref={inviteTeamsModalRef} />
    </>
  );
};

export default TournamentDetails;
