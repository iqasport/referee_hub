import React, { useRef, useState } from "react";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ContactOrganizerModal, { ContactOrganizerModalRef } from "./ContactOrganizerModal";
import AddTournamentModal, { AddTournamentModalRef } from "../components/AddTournamentModal";
import RegistrationsModal, { RegistrationsModalRef } from "./RegistrationsModal";
import InviteTeamsModal, { InviteTeamsModalRef } from "./InviteTeamsModal";
import AddTournamentManagerModal from "./AddTournamentManagerModal";
import ActionButtonPair from "../../../components/ActionButtonPair";
import CustomAlert, { AlertType } from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import {
  TournamentHeader,
  TournamentInfoCards,
  TournamentAboutSection,
  RosterManager,
} from "./components";
import { TournamentDetailsSidebar } from "./components/TournamentDetailsSidebar";
import { useNavigationParams } from "../../../utils/navigationUtils";
import {
  useTournamentPermissions,
  useApprovedTeams,
  usePendingInvites,
  useIsRegistrationClosed,
  useManagedTeamIds,
  useRosterStats,
  useTournamentDetailsData,
  useTournamentDateRange,
  useTournamentEditHandler,
  useDeleteTournamentHandler,
  useRespondToInviteHandler,
} from "./hooks";

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
    type: AlertType;
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
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);
  const rosterSectionRef = useRef<HTMLDivElement>(null);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [isAddManagerModalOpen, setIsAddManagerModalOpen] = useState(false);
  const { alertState, showAlert, hideAlert } = useAlert();

  // Load all tournament data with a single hook
  const {
    currentUser,
    tournament,
    managers,
    invites,
    participants,
    managedTeamsData,
    isAnonymous,
    isTournamentManager,
    isLoading,
    isError,
    respondToInvite,
    refetchInvites,
    refetchParticipants,
  } = useTournamentDetailsData(tournamentId);

  // Extract managed team IDs
  const managedTeamIds = useManagedTeamIds(managedTeamsData);

  // Extract pending and approved invites
  const pendingInvitesForUser = usePendingInvites(invites, managedTeamIds);
  const approvedTeamsForUser = useApprovedTeams(invites, managedTeamIds, managedTeamsData);

  // Extract roster stats
  const { totalPlayerCount } = useRosterStats(participants);

  // Check if registration is closed
  const isRegistrationClosed = useIsRegistrationClosed(tournament);

  // Handle date range formatting
  const formattedDateRange = useTournamentDateRange(tournament?.startDate, tournament?.endDate);

  // Handle edit tournament (for managers)
  const { editModalRef, handleEdit } = useTournamentEditHandler(tournament);

  // Handle delete tournament
  const { handleDelete } = useDeleteTournamentHandler({
    tournamentId,
    tournamentName: tournament?.name,
    showAlert,
  });

  // Handle invite response
  const { handleRespondToInvite } = useRespondToInviteHandler({
    tournamentId,
    respondingTo,
    setRespondingTo,
    refetchInvites,
    showAlert,
    respondToInviteMutation: respondToInvite,
  });

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

  const { handleEdit } = useTournamentEditHandler(tournament);
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
              <TournamentDetailsSidebar
                isTournamentManager={isTournamentManager}
                isAnonymous={isAnonymous}
                isRegistrationClosed={isRegistrationClosed}
                pendingInvitesForUser={pendingInvitesForUser}
                approvedTeamsForUser={approvedTeamsForUser}
                respondingTo={respondingTo}
                onRespondToInvite={handleRespondToInvite}
                onEdit={handleEdit}
                onOpenRegistrations={() =>
                  registrationsModalRef.current?.open(
                    tournament.id || "",
                    tournament.name || "Unknown Tournament"
                  )
                }
                onInviteTeams={() => inviteTeamsModalRef.current?.open(tournament)}
                onAddManager={() => setIsAddManagerModalOpen(true)}
                onDelete={handleDelete}
                onContactOrganizer={() =>
                  contactOrganizerModalRef.current?.open({
                    name: tournament.organizer || "",
                    tournamentName: tournament.name || "",
                    tournamentId: tournament.id,
                  })
                }
                onRegister={() =>
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
                onManageRosters={() => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" })}
                totalPlayerCount={totalPlayerCount}
                invitesCount={invites?.filter((i) => i.status === "approved").length || 0}
                isPrivate={tournament.isPrivate || false}
              />
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
