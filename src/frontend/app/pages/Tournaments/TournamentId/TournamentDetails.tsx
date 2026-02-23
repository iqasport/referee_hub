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
  TournamentViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

// ── Helpers ──────────────────────────────────────────────────────────────────

function buildTournamentEditPayload(tournament: TournamentViewModel) {
  return {
    id: tournament.id || "",
    name: tournament.name || "",
    description: tournament.description || "",
    startDate: tournament.startDate || "",
    endDate: tournament.endDate || "",
    registrationEndsDate: tournament.registrationEndsDate || "",
    type: (tournament.type || "") as "" | import("../../../store/serviceApi").TournamentType,
    country: tournament.country || "",
    city: tournament.city || "",
    place: tournament.place || "",
    organizer: tournament.organizer || "",
    isPrivate: tournament.isPrivate || false,
    isRegistrationOpen: tournament.isRegistrationOpen ?? true,
    allowsIndividualRegistration: tournament.allowsIndividualRegistration ?? false,
    allowsTeamRegistration: tournament.allowsTeamRegistration ?? true,
    bannerImageUrl: tournament.bannerImageUrl || "",
  };
}

function buildRegisterModalPayload(tournament: TournamentViewModel) {
  return {
    id: tournament.id || "",
    name: tournament.name || "",
    startDate: tournament.startDate || "",
    endDate: tournament.endDate || "",
    country: tournament.country || "",
    city: tournament.city || "",
    type: tournament.type || "",
    allowsIndividualRegistration: tournament.allowsIndividualRegistration,
    allowsTeamRegistration: tournament.allowsTeamRegistration,
  };
}

// ── Sub-components ────────────────────────────────────────────────────────────

interface PendingInvitesCardProps {
  pendingInvites: TournamentInviteViewModel[];
  respondingTo: string | null;
  isManager: boolean;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
}
const PendingInvitesCard: React.FC<PendingInvitesCardProps> = ({
  pendingInvites, respondingTo, isManager, onAccept, onDecline,
}) => (
  <div className="card card-highlighted card-mb">
    <h3 className="card-title">You&apos;re Invited!</h3>
    <p className="card-description">
      {isManager
        ? "Your team(s) have been invited to participate in this tournament."
        : "The tournament organizer has invited your team(s) to participate."}
    </p>
    <div className="invite-list">
      {pendingInvites.map((invite) => (
        <div key={invite.participantId} className="invite-item">
          <p className="invite-team-name">{invite.participantName}</p>
          <ActionButtonPair
            onAccept={() => onAccept(invite.participantId || "")}
            onDecline={() => onDecline(invite.participantId || "")}
            isLoading={respondingTo === invite.participantId}
            size="sm"
          />
        </div>
      ))}
    </div>
  </div>
);

interface ApprovedTeam { teamId?: string | null; teamName: string; ngb: string }

interface ManagerTeamCardProps {
  isRegistrationClosed: boolean;
  approvedTeams: ApprovedTeam[];
  onScrollToRoster: () => void;
  onOpenRegisterModal: () => void;
}
const ManagerTeamCard: React.FC<ManagerTeamCardProps> = ({
  isRegistrationClosed, approvedTeams, onScrollToRoster, onOpenRegisterModal,
}) => {
  if (isRegistrationClosed) {
    return (
      <div className="card card-mb">
        {approvedTeams.length > 0 ? (
          <>
            <h3 className="card-title">Your Teams Are Registered</h3>
            <p className="card-description">Your team(s) are registered. Manage your rosters below.</p>
            <button onClick={onScrollToRoster} className="btn btn-primary btn-full-width">
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
    );
  }
  if (approvedTeams.length > 0) {
    return (
      <div className="card card-mb">
        <h3 className="card-title">Your Teams Are Registered!</h3>
        <p className="card-description">Your team(s) are registered. Manage your rosters or register another team.</p>
        <button onClick={onScrollToRoster} className="btn btn-primary btn-full-width card-mb">
          Manage Your Rosters
        </button>
        <button onClick={onOpenRegisterModal} className="btn btn-outline btn-full-width">
          Register Another Team
        </button>
      </div>
    );
  }
  return (
    <div className="card card-mb">
      <h3 className="card-title">Register Your Team</h3>
      <p className="card-description">You also manage teams. You can register them for this tournament.</p>
      <button onClick={onOpenRegisterModal} className="btn btn-outline btn-full-width">
        Register a Team
      </button>
    </div>
  );
};

interface UserRegistrationCardProps {
  isRegistrationClosed: boolean;
  approvedTeams: ApprovedTeam[];
  myIndividualInvite: TournamentInviteViewModel | null;
  onScrollToRoster: () => void;
  onOpenRegisterModal: () => void;
}
const UserRegistrationCard: React.FC<UserRegistrationCardProps> = ({
  isRegistrationClosed, approvedTeams, myIndividualInvite, onScrollToRoster, onOpenRegisterModal,
}) => {
  if (isRegistrationClosed) {
    return (
      <div className="card card-mb">
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
      </div>
    );
  }
  if (approvedTeams.length > 0) {
    return (
      <div className="card card-mb">
        <h3 className="card-title">You&apos;re Registered!</h3>
        <p className="card-description">Your team is registered for this tournament. Manage your roster below.</p>
        <button onClick={onScrollToRoster} className="btn btn-primary btn-full-width card-mb">
          Manage Your Rosters
        </button>
        <button onClick={onOpenRegisterModal} className="btn btn-outline btn-full-width">
          Register Another Team
        </button>
      </div>
    );
  }
  if (myIndividualInvite) {
    const statusCls =
      myIndividualInvite.status === "approved" ? "bg-green-50 border border-green-200 text-green-800"
      : myIndividualInvite.status === "rejected" ? "bg-red-50 border border-red-200 text-red-800"
      : "bg-yellow-50 border border-yellow-200 text-yellow-800";
    return (
      <div className="card card-mb">
        <h3 className="card-title">
          {myIndividualInvite.status === "approved" ? "You're Registered!"
            : myIndividualInvite.status === "rejected" ? "Registration Rejected"
            : "Registration Pending"}
        </h3>
        <div className={`mt-2 p-3 rounded text-sm font-medium ${statusCls}`}>
          {myIndividualInvite.status === "approved" && "You have been accepted to this tournament as an individual player."}
          {myIndividualInvite.status === "rejected" && "Your individual registration was rejected by the tournament organizer."}
          {(myIndividualInvite.status === "pending" || !myIndividualInvite.status) && "Your registration is pending the tournament organizer's decision."}
        </div>
      </div>
    );
  }
  return (
    <div className="card card-mb">
      <h3 className="card-title">Register Now</h3>
      <p className="card-description">Secure your spot in this exciting tournament. Limited slots available!</p>
      <button onClick={onOpenRegisterModal} className="btn btn-primary btn-full-width">
        Register for Tournament
      </button>
    </div>
  );
};

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

  const [addTournamentManager, { isLoading: isAddingManager }] = useAddTournamentManagerMutation();

  const {
    data: tournament,
    isLoading,
    isError,
  } = useGetTournamentQuery({ tournamentId: tournamentId || "" });
  const { data: currentUser } = useGetCurrentUserQuery();

  // Use the new managed teams endpoint to get teams the user manages
  const { data: managedTeamsData } = useGetManagedTeamsQuery();

  // Check if user is a tournament manager for this specific tournament
  // Note: role.tournament can be "ANY", a single tournament ID string, or an array of tournament IDs
  const isTournamentManagerOfThis = currentUser?.roles?.some((role: any) => {
    if (role.roleType !== "TournamentManager") return false;
    if (role.tournament === "ANY") return true;
    if (Array.isArray(role.tournament)) {
      return role.tournament.includes(tournamentId);
    }
    return role.tournament === tournamentId;
  });

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
      // Check if this invite is for one of user's teams
      if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;

      // Check if participant approval is pending (user needs to respond)
      return invite.participantApproval?.status === "pending";
    });
  }, [invites, managedTeamIds]);

  // Find teams that are fully approved and participating
  const approvedTeamsForUser = useMemo(() => {
    if (!invites || managedTeamIds.size === 0 || !managedTeamsData) return [];

    return invites
      .filter((invite) => {
        // Check if this invite is for one of user's teams
        if (!invite.participantId || !managedTeamIds.has(invite.participantId)) return false;
        // Check if the invite is fully approved
        return invite.status === "approved";
      })
      .map((invite) => {
        const teamData = managedTeamsData.find((t) => t.teamId === invite.participantId);
        return {
          teamId: invite.participantId,
          teamName: invite.participantName || teamData?.teamName || "Unknown Team",
          ngb: teamData?.ngb || "",
        };
      });
  }, [invites, managedTeamIds, managedTeamsData]);

  // Detect if the current user has an individual (player) invite for this tournament.
  // Used to replace the "Register Now" card with a status-aware notice for individual players.
  const myIndividualInvite = useMemo(() => {
    if (!invites || !currentUser?.userId) return null;
    return invites.find(
      (i) => i.participantType === "player" && i.participantId === currentUser.userId
    ) ?? null;
  }, [invites, currentUser?.userId]);

  // Calculate team count (number of teams registered)
  const teamCount = useMemo(() => {
    if (!participants) return 0;
    return participants.length;
  }, [participants]);

  // Calculate total participant count from all team rosters (players + coaches + staff)
  const totalParticipantCount = useMemo(() => {
    if (!participants) return 0;
    return participants.reduce((total, team) => {
      const playerCount = team.players?.length || 0;
      const coachCount = team.coaches?.length || 0;
      const staffCount = team.staff?.length || 0;
      return total + playerCount + coachCount + staffCount;
    }, 0);
  }, [participants]);
  
  // Calculate total player count (excluding coaches and staff)
  const totalPlayerCount = useMemo(() => {
    if (!participants) return 0;
    return participants.reduce((total, team) => {
      const playerCount = team.players?.length || 0;
      return total + playerCount;
    }, 0);
  }, [participants]);

  // Determine if registration is closed (manual toggle or date-based)
  const isRegistrationClosed = useMemo(() => {
    // Check manual closure first (field may not exist if migration not applied)
    if (tournament?.isRegistrationOpen === false) {
      return true;
    }

    // Check if registration end date has passed
    if (tournament?.registrationEndsDate) {
      const regEndsDate = new Date(tournament.registrationEndsDate);
      const today = new Date();
      // Reset hours to compare at day level
      regEndsDate.setHours(0, 0, 0, 0);
      today.setHours(0, 0, 0, 0);
      if (today > regEndsDate) {
        return true;
      }
    } else if (tournament?.startDate) {
      // Fall back to start date if no registration end date
      const startDate = new Date(tournament.startDate);
      const today = new Date();
      // Reset hours to compare at day level
      startDate.setHours(0, 0, 0, 0);
      today.setHours(0, 0, 0, 0);
      if (today > startDate) {
        return true;
      }
    }

    return false;
  }, [tournament?.isRegistrationOpen, tournament?.registrationEndsDate, tournament?.startDate]);

  // Handle add manager
  async function handleAddManager(e: React.FormEvent) {
    e.preventDefault();
    if (!addManagerEmail.trim() || !tournamentId) return;
    try {
      await addTournamentManager({
        tournamentId,
        addTournamentManagerModel: { email: addManagerEmail.trim() },
      }).unwrap();
      showAlert(`Successfully added manager.`, "success");
      setAddManagerEmail("");
      setIsAddManagerOpen(false);
    } catch (error) {
      console.error("Failed to add manager:", error);
      showAlert("Failed to add manager. Check that the email belongs to a registered user.", "error");
    }
  }

  function handleCancelAddManager() {
    setIsAddManagerOpen(false);
    setAddManagerEmail("");
  }

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

  // Check if current user is a manager of this tournament
  // Only consider them a manager if they're in the managers list and we successfully fetched the list
  const isManager =
    !managersError && currentUser?.userId && managers
      ? managers.some((manager) => manager.id === currentUser.userId)
      : false;

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

  const handleEdit = () => editModalRef.current?.openEdit(buildTournamentEditPayload(tournament));
  const openRegisterModal = () => registerModalRef.current?.open(buildRegisterModalPayload(tournament));

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
              {isManager ? (
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
                      onClick={() => setIsAddManagerOpen(true)}
                      className="btn btn-secondary btn-full-width"
                    >
                      Add Tournament Manager
                    </button>
                  </div>

                  {/* Add Manager Inline Form */}
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
                          <button
                            type="submit"
                            disabled={isAddingManager || !addManagerEmail.trim()}
                            className="btn btn-primary"
                          >
                            {isAddingManager ? "Adding..." : "Add Manager"}
                          </button>
                          <button
                            type="button"
                            onClick={handleCancelAddManager}
                            className="btn btn-secondary"
                          >
                            Cancel
                          </button>
                        </div>
                      </form>
                    </div>
                  )}

                  {/* Tournament Stats Card */}
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

                  {/* Manager who also manages teams: pending invites they need to respond to */}
                  {pendingInvitesForUser.length > 0 && (
                    <PendingInvitesCard
                      pendingInvites={pendingInvitesForUser}
                      respondingTo={respondingTo}
                      isManager={true}
                      onAccept={(id) => handleRespondToInvite(id, true)}
                      onDecline={(id) => handleRespondToInvite(id, false)}
                    />
                  )}

                  {/* Manager who also manages teams: register / roster section */}
                  {managedTeamsData && managedTeamsData.length > 0 && (
                    <ManagerTeamCard
                      isRegistrationClosed={isRegistrationClosed}
                      approvedTeams={approvedTeamsForUser}
                      onScrollToRoster={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })}
                      onOpenRegisterModal={openRegisterModal}
                    />
                  )}
                </>
              ) : (
                <>
                  {pendingInvitesForUser.length > 0 && (
                    <PendingInvitesCard
                      pendingInvites={pendingInvitesForUser}
                      respondingTo={respondingTo}
                      isManager={false}
                      onAccept={(id) => handleRespondToInvite(id, true)}
                      onDecline={(id) => handleRespondToInvite(id, false)}
                    />
                  )}

                  {/* Register Now / Manage Rosters Card */}
                  <UserRegistrationCard
                    isRegistrationClosed={isRegistrationClosed}
                    approvedTeams={approvedTeamsForUser}
                    myIndividualInvite={myIndividualInvite}
                    onScrollToRoster={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })}
                    onOpenRegisterModal={openRegisterModal}
                  />

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
