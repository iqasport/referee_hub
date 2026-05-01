import React, { useRef, useMemo, useState } from "react";
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

type TeamSummary = {
  teamId: string;
  teamName: string;
  ngb: string;
};

type ManagerSidebarProps = {
  tournament: any;
  invites?: TournamentInviteViewModel[];
  totalPlayerCount: number;
  onEdit: () => void;
  onOpenRegistrations: () => void;
  onOpenInviteTeams: () => void;
  onOpenAddManager: () => void;
  onDelete: () => void;
};

const ManagerSidebar = ({
  tournament,
  invites,
  totalPlayerCount,
  onEdit,
  onOpenRegistrations,
  onOpenInviteTeams,
  onOpenAddManager,
  onDelete,
}: ManagerSidebarProps) => (
  <>
    <div className="card card-highlighted card-mb card-sticky">
      <h3 className="card-title">Manager Tools</h3>
      <p className="card-description">
        You are the manager of this tournament. Use the tools below to manage the tournament.
      </p>
      <button onClick={onEdit} className="btn btn-primary btn-full-width btn-with-icon card-mb">
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
      <button onClick={onOpenRegistrations} className="btn btn-secondary btn-full-width card-mb">
        View Team Registrations ({invites?.length || 0})
      </button>
      <button onClick={onOpenInviteTeams} className="btn btn-secondary btn-full-width card-mb">
        Invite Teams
      </button>
      <button onClick={onOpenAddManager} className="btn btn-secondary btn-full-width">
        Add Tournament Manager
      </button>
      <button onClick={onDelete} className="btn btn-danger btn-full-width" style={{ marginTop: "0.75rem" }}>
        Delete Tournament
      </button>
    </div>

    <div className="card">
      <h3 className="card-title">Tournament Stats</h3>
      <div className="stats-list">
        <div className="stats-item">
          <span className="stats-label">Teams Registered</span>
          <span className="stats-value">{invites?.filter((i) => i.status === "approved").length || 0}</span>
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
);

type UserSidebarProps = {
  isRegistrationClosed: boolean;
  pendingInvitesForUser: TournamentInviteViewModel[];
  approvedTeamsForUser: TeamSummary[];
  respondingTo: string | null;
  onRespondToInvite: (participantId: string, approved: boolean) => void;
  onScrollToRosters: () => void;
  onOpenRegister: () => void;
  onOpenContactOrganizer: () => void;
};

const UserSidebar = ({
  isRegistrationClosed,
  pendingInvitesForUser,
  approvedTeamsForUser,
  respondingTo,
  onRespondToInvite,
  onScrollToRosters,
  onOpenRegister,
  onOpenContactOrganizer,
}: UserSidebarProps) => (
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
                onAccept={() => onRespondToInvite(invite.participantId || "", true)}
                onDecline={() => onRespondToInvite(invite.participantId || "", false)}
                isLoading={respondingTo === invite.participantId}
                size="sm"
              />
            </div>
          ))}
        </div>
      </div>
    )}

    <div className="card card-mb">
      {isRegistrationClosed ? (
        <>
          <h3 className="card-title">Registration Closed</h3>
          <p className="card-description">
            Registration for this tournament is now closed. Contact the organizers if you have
            questions.
          </p>
          <div className="mt-4 p-3 bg-gray-100 rounded-md border border-gray-300 text-center">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
              />
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
          <button onClick={onScrollToRosters} className="btn btn-primary btn-full-width card-mb">
            Manage Your Rosters
          </button>
          <button onClick={onOpenRegister} className="btn btn-outline btn-full-width">
            Register Another Team
          </button>
        </>
      ) : (
        <>
          <h3 className="card-title">Register Now</h3>
          <p className="card-description">
            Secure your spot in this exciting tournament. Limited slots available!
          </p>
          <button onClick={onOpenRegister} className="btn btn-primary btn-full-width">
            Register for Tournament
          </button>
        </>
      )}
    </div>

    <div className="card">
      <h3 className="card-title">Need Help?</h3>
      <p className="card-description">Have questions about this tournament? Contact the organizers.</p>
      <button onClick={onOpenContactOrganizer} className="btn btn-outline btn-full-width">
        Contact Organizer
      </button>
    </div>
  </>
);

type TournamentDetailsContentProps = {
  alertState: {
    isVisible: boolean;
    message: string;
    type: "success" | "error";
  };
  hideAlert: () => void;
  tournament: any;
  isManager: boolean;
  formattedDateRange: string;
  invites?: TournamentInviteViewModel[];
  totalPlayerCount: number;
  isRegistrationClosed: boolean;
  pendingInvitesForUser: TournamentInviteViewModel[];
  approvedTeamsForUser: TeamSummary[];
  respondingTo: string | null;
  rosterSectionRef: React.RefObject<HTMLDivElement>;
  tournamentId: string;
  onEdit: () => void;
  onOpenRegistrations: () => void;
  onOpenInviteTeams: () => void;
  onOpenAddManager: () => void;
  onDelete: () => void;
  onRespondToInvite: (participantId: string, approved: boolean) => void;
  onScrollToRosters: () => void;
  onOpenRegister: () => void;
  onOpenContactOrganizer: () => void;
  onRosterSaved: () => void;
};

const TournamentDetailsContent = ({
  alertState,
  hideAlert,
  tournament,
  isManager,
  formattedDateRange,
  invites,
  totalPlayerCount,
  isRegistrationClosed,
  pendingInvitesForUser,
  approvedTeamsForUser,
  respondingTo,
  rosterSectionRef,
  tournamentId,
  onEdit,
  onOpenRegistrations,
  onOpenInviteTeams,
  onOpenAddManager,
  onDelete,
  onRespondToInvite,
  onScrollToRosters,
  onOpenRegister,
  onOpenContactOrganizer,
  onRosterSaved,
}: TournamentDetailsContentProps) => (
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
              <ManagerSidebar
                tournament={tournament}
                invites={invites}
                totalPlayerCount={totalPlayerCount}
                onEdit={onEdit}
                onOpenRegistrations={onOpenRegistrations}
                onOpenInviteTeams={onOpenInviteTeams}
                onOpenAddManager={onOpenAddManager}
                onDelete={onDelete}
              />
            ) : (
              <UserSidebar
                isRegistrationClosed={isRegistrationClosed}
                pendingInvitesForUser={pendingInvitesForUser}
                approvedTeamsForUser={approvedTeamsForUser}
                respondingTo={respondingTo}
                onRespondToInvite={onRespondToInvite}
                onScrollToRosters={onScrollToRosters}
                onOpenRegister={onOpenRegister}
                onOpenContactOrganizer={onOpenContactOrganizer}
              />
            )}
          </div>
        </div>

        {/* Roster Management Section - Show for team managers with approved teams */}
        {approvedTeamsForUser.length > 0 && (
          <div ref={rosterSectionRef} className="roster-section">
            <h2 className="card-title card-title-lg">Manage Your Team Rosters</h2>
            <RosterManager
              tournamentId={tournamentId}
              teams={approvedTeamsForUser}
              onRosterSaved={onRosterSaved}
            />
          </div>
        )}
      </div>
    </section>
  </>
);

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
  const [deleteTournament] = useDeleteTournamentMutation();
  const navigate = useNavigate();

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

  const handleOpenRegister = () => {
    registerModalRef.current?.open({
      id: tournament.id || "",
      name: tournament.name || "",
      startDate: tournament.startDate || "",
      endDate: tournament.endDate || "",
      country: tournament.country || "",
      city: tournament.city || "",
      type: tournament.type || "",
    });
  };

  const handleOpenContactOrganizer = () => {
    contactOrganizerModalRef.current?.open({
      name: tournament.organizer || "",
      tournamentName: tournament.name || "",
      tournamentId: tournament.id,
    });
  };

  return (
    <>
      <TournamentDetailsContent
        alertState={alertState}
        hideAlert={hideAlert}
        tournament={tournament}
        isManager={isManager}
        formattedDateRange={formattedDateRange}
        invites={invites}
        totalPlayerCount={totalPlayerCount}
        isRegistrationClosed={isRegistrationClosed}
        pendingInvitesForUser={pendingInvitesForUser}
        approvedTeamsForUser={approvedTeamsForUser}
        respondingTo={respondingTo}
        rosterSectionRef={rosterSectionRef}
        tournamentId={tournamentId || ""}
        onEdit={handleEdit}
        onOpenRegistrations={() =>
          registrationsModalRef.current?.open(
            tournament.id || "",
            tournament.name || "Unknown Tournament"
          )
        }
        onOpenInviteTeams={() => inviteTeamsModalRef.current?.open(tournament)}
        onOpenAddManager={() => setIsAddManagerModalOpen(true)}
        onDelete={handleDelete}
        onRespondToInvite={handleRespondToInvite}
        onScrollToRosters={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })}
        onOpenRegister={handleOpenRegister}
        onOpenContactOrganizer={handleOpenContactOrganizer}
        onRosterSaved={() => {
          refetchInvites();
          refetchParticipants();
        }}
      />

      {/* Regular user modals */}
      <RegisterTournamentModal ref={registerModalRef} />
      <ContactOrganizerModal ref={contactOrganizerModalRef} />

      {/* Manager modals */}
      <AddTournamentModal ref={editModalRef} />
      <RegistrationsModal ref={registrationsModalRef} />
      <InviteTeamsModal ref={inviteTeamsModalRef} />
      {isAddManagerModalOpen && tournamentId && (
        <AddTournamentManagerModal
          tournamentId={tournamentId}
          onClose={() => setIsAddManagerModalOpen(false)}
        />
      )}
    </>
  );
};

export default TournamentDetails;
