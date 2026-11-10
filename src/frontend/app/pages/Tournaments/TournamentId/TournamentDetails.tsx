import React, { useMemo, useRef, useState } from "react";
import RegisterTournamentModal, {
  RegisterTournamentModalRef,
} from "./RegisterTournamentModal";
import ContactOrganizerModal, {
  ContactOrganizerModalRef,
} from "./ContactOrganizerModal";
import AddTournamentModal, {
  AddTournamentModalRef,
} from "../components/AddTournamentModal";
import TeamRegistrationsModal, {
  TeamRegistrationsModalRef,
} from "./TeamRegistrationsModal";
import VolunteerRegistrationsModal, {
  VolunteerRegistrationsModalRef,
} from "./VolunteerRegistrationsModal";
import VolunteerRegistrationModal, {
  VolunteerRegistrationModalRef,
} from "./VolunteerRegistrationModal";
import InviteTeamsModal, {
  InviteTeamsModalRef,
} from "./InviteTeamsModal";
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
import { TournamentInviteViewModel } from "../../../store/serviceApi";
import { useTournamentDetailsData } from "./hooks/useTournamentDetailsData";
import {
  useNavigationParams,
  useNavigate,
} from "../../../utils/navigationUtils";

type TeamSummary = {
  teamId: string;
  teamName: string;
  ngb: string;
};

const valueOrDefault = <T,>(value: T | null | undefined, fallback: T): T =>
  value ?? fallback;

const getManagedTeamIds = (teams?: { teamId?: string | null }[]) => {
  const teamIds = new Set<string>();
  teams?.forEach((team) => {
    if (team.teamId) teamIds.add(team.teamId);
  });
  return teamIds;
};

const getPendingInvitesForManagedTeams = (
  invites: TournamentInviteViewModel[] | undefined,
  managedTeamIds: Set<string>
) => {
  if (!invites || managedTeamIds.size === 0) return [];
  return invites.filter((invite) => {
    const teamId = invite.participantId;
    return Boolean(
      teamId
      && managedTeamIds.has(teamId)
      && invite.participantApproval?.status === "pending"
    );
  });
};

const getApprovedTeamsForManagedTeams = (
  invites: TournamentInviteViewModel[] | undefined,
  teams: { teamId?: string | null; teamName?: string | null; ngb?: string | null }[] | undefined,
  managedTeamIds: Set<string>
): TeamSummary[] => {
  if (!invites || !teams || managedTeamIds.size === 0) return [];
  return invites.reduce<TeamSummary[]>((approvedTeams, invite) => {
    const teamId = invite.participantId;
    if (!teamId || !managedTeamIds.has(teamId) || invite.status !== "approved") {
      return approvedTeams;
    }
    const teamData = teams.find((team) => team.teamId === teamId);
    approvedTeams.push({
      teamId,
      teamName: invite.participantName || teamData?.teamName || "Unknown Team",
      ngb: teamData?.ngb || "",
    });
    return approvedTeams;
  }, []);
};

const getTotalPlayerCount = (participants?: { players?: unknown[] | null }[]) =>
  participants?.reduce((total, team) => total + (team.players?.length || 0), 0) || 0;

const isRegistrationClosed = (
  registrationIsOpen?: boolean,
  registrationEndsDate?: string,
  startDate?: string
) => {
  if (registrationIsOpen === false) return true;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const closingDate = registrationEndsDate || startDate;
  if (!closingDate) return false;
  const normalizedClosingDate = new Date(closingDate);
  normalizedClosingDate.setHours(0, 0, 0, 0);
  return today > normalizedClosingDate;
};

const formatTournamentDateRange = (startDate: Date, endDate: Date) => {
  if (startDate.toDateString() === endDate.toDateString()) {
    return startDate.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  }
  return `${startDate.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  })} - ${endDate.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  })}`;
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
  onOpenVolunteerReview: () => void;
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
  onOpenVolunteerReview,
}: ManagerSidebarProps) => (
  <>
    <div className="card card-highlighted card-mb card-sticky">
      <h3 className="card-title">Manager Tools</h3>

      <p className="card-description">
        You are the manager of this tournament. Use the tools below to manage
        the tournament.
      </p>

      <button
        onClick={onEdit}
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
        onClick={onOpenRegistrations}
        className="btn btn-secondary btn-full-width card-mb"
      >
        View Team Registrations (
        {invites?.filter((invite) => invite.participantType === "team")
          .length || 0}
        )
      </button>

      <button
        onClick={onOpenVolunteerReview}
        className="btn btn-secondary btn-full-width card-mb"
      >
        Review Volunteer Applications (
        {invites?.filter((invite) => invite.participantType === "referee")
          .length || 0}
        )
      </button>

      <button
        onClick={onOpenInviteTeams}
        className="btn btn-secondary btn-full-width card-mb"
      >
        Invite Teams
      </button>

      <button
        onClick={onOpenAddManager}
        className="btn btn-secondary btn-full-width"
      >
        Add Tournament Manager
      </button>

      <button
        onClick={onDelete}
        className="btn btn-danger btn-full-width"
        style={{ marginTop: "0.75rem" }}
      >
        Delete Tournament
      </button>
    </div>

    <div className="card">
      <h3 className="card-title">Tournament Stats</h3>

      <div className="stats-list">
        <div className="stats-item">
          <span className="stats-label">Teams Registered</span>
          <span className="stats-value">
            {invites?.filter((invite) => invite.status === "approved")
              .length || 0}
          </span>
        </div>

        <div className="stats-item">
          <span className="stats-label">Players Registered</span>
          <span className="stats-value">{totalPlayerCount}</span>
        </div>

        <div className="stats-item">
          <span className="stats-label">Private Tournament</span>
          <span className="stats-value">
            {tournament.isPrivate ? "Yes" : "No"}
          </span>
        </div>
      </div>
    </div>
  </>
);

type UserSidebarProps = {
  isRegistrationClosed: boolean;
  isVolunteerRegistrationOpen: boolean;
  pendingInvitesForUser: TournamentInviteViewModel[];
  approvedTeamsForUser: TeamSummary[];
  respondingTo: string | null;
  onRespondToInvite: (
    participantId: string,
    approved: boolean
  ) => void;
  onScrollToRosters: () => void;
  onOpenRegister: () => void;
  onOpenContactOrganizer: () => void;
  onOpenVolunteerForm: () => void;
};

const UserSidebar = ({
  isRegistrationClosed,
  isVolunteerRegistrationOpen,
  pendingInvitesForUser,
  approvedTeamsForUser,
  respondingTo,
  onRespondToInvite,
  onScrollToRosters,
  onOpenRegister,
  onOpenContactOrganizer,
  onOpenVolunteerForm,
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
                onAccept={() =>
                  onRespondToInvite(invite.participantId || "", true)
                }
                onDecline={() =>
                  onRespondToInvite(invite.participantId || "", false)
                }
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
            Registration for this tournament is now closed. Contact the
            organizers if you have questions.
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

            <p className="text-sm font-medium text-gray-700">
              Registration is now closed
            </p>
          </div>
        </>
      ) : approvedTeamsForUser.length > 0 ? (
        <>
          <h3 className="card-title">You&apos;re Registered!</h3>

          <p className="card-description">
            Your team is registered for this tournament. Manage your roster
            below.
          </p>

          <button
            onClick={onScrollToRosters}
            className="btn btn-primary btn-full-width card-mb"
          >
            Manage Your Rosters
          </button>

          <button
            onClick={onOpenRegister}
            className="btn btn-outline btn-full-width"
          >
            Register Another Team
          </button>
        </>
      ) : (
        <>
          <h3 className="card-title">Register Now</h3>

          <p className="card-description">
            Secure your spot in this exciting tournament. Limited slots
            available!
          </p>

          <button
            onClick={onOpenRegister}
            className="btn btn-primary btn-full-width"
          >
            Register for Tournament
          </button>
        </>
      )}
    </div>

    {isVolunteerRegistrationOpen && (
      <div className="card card-mb">
        <h3 className="card-title">Volunteer as a Referee</h3>

        <p className="card-description">
          Register yourself as a volunteer referee for this tournament.
        </p>

        <button
          onClick={onOpenVolunteerForm}
          className="btn btn-secondary btn-full-width"
        >
          Register as Volunteer
        </button>
      </div>
    )}

    <div className="card">
      <h3 className="card-title">Need Help?</h3>

      <p className="card-description">
        Have questions about this tournament? Contact the organizers.
      </p>

      <button
        onClick={onOpenContactOrganizer}
        className="btn btn-outline btn-full-width"
      >
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
  onRespondToInvite: (
    participantId: string,
    approved: boolean
  ) => void;
  onScrollToRosters: () => void;
  onOpenRegister: () => void;
  onOpenContactOrganizer: () => void;
  onRosterSaved: () => void;
  onOpenVolunteerForm: () => void;
  onOpenVolunteerReview: () => void;
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
  onOpenVolunteerForm,
  onOpenVolunteerReview,
}: TournamentDetailsContentProps) => (
  <>
    {alertState.isVisible && (
      <CustomAlert
        message={alertState.message}
        type={alertState.type}
        onClose={hideAlert}
      />
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
              <ManagerSidebar
                tournament={tournament}
                invites={invites}
                totalPlayerCount={totalPlayerCount}
                onEdit={onEdit}
                onOpenRegistrations={onOpenRegistrations}
                onOpenVolunteerReview={onOpenVolunteerReview}
                onOpenInviteTeams={onOpenInviteTeams}
                onOpenAddManager={onOpenAddManager}
                onDelete={onDelete}
              />
            ) : (
              <UserSidebar
                isRegistrationClosed={isRegistrationClosed}
                isVolunteerRegistrationOpen={
                  tournament.isVolunteerRegistrationOpen
                }
                pendingInvitesForUser={pendingInvitesForUser}
                approvedTeamsForUser={approvedTeamsForUser}
                respondingTo={respondingTo}
                onRespondToInvite={onRespondToInvite}
                onScrollToRosters={onScrollToRosters}
                onOpenRegister={onOpenRegister}
                onOpenVolunteerForm={onOpenVolunteerForm}
                onOpenContactOrganizer={onOpenContactOrganizer}
              />
            )}
          </div>
        </div>

        {approvedTeamsForUser.length > 0 && (
          <div ref={rosterSectionRef} className="roster-section">
            <h2 className="card-title card-title-lg">
              Manage Your Team Rosters
            </h2>

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
  const { tournamentId } =
    useNavigationParams<"tournamentId">();

  const registerModalRef =
    useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef =
    useRef<ContactOrganizerModalRef>(null);
  const editModalRef =
    useRef<AddTournamentModalRef>(null);

  const teamRegistrationsModalRef =
    useRef<TeamRegistrationsModalRef>(null);
  const volunteerReviewModalRef =
    useRef<VolunteerRegistrationsModalRef>(null);
  const volunteerFormModalRef =
    useRef<VolunteerRegistrationModalRef>(null);

  const inviteTeamsModalRef =
    useRef<InviteTeamsModalRef>(null);
  const rosterSectionRef =
    useRef<HTMLDivElement>(null);

  const [isAddManagerModalOpen, setIsAddManagerModalOpen] =
    useState(false);

  const { alertState, showAlert, hideAlert } = useAlert();
  const navigate = useNavigate();

  const {
    currentUser,
    isTournamentManager: isManager,
    isLoading,
    isError,
    respondingTo,
    setRespondingTo,
    tournament,
    invites,
    participants,
    managedTeamsData,
    respondToInvite,
    deleteTournament,
    refetchInvites,
    refetchParticipants,
  } = useTournamentDetailsData(tournamentId);

  const managedTeamIds = useMemo(
    () => getManagedTeamIds(managedTeamsData),
    [managedTeamsData]
  );

  const pendingInvitesForUser = useMemo(
    () => getPendingInvitesForManagedTeams(invites, managedTeamIds),
    [invites, managedTeamIds]
  );

  const approvedTeamsForUser = useMemo(
    () => getApprovedTeamsForManagedTeams(invites, managedTeamsData, managedTeamIds),
    [invites, managedTeamsData, managedTeamIds]
  );

  const totalPlayerCount = useMemo(
    () => getTotalPlayerCount(participants),
    [participants]
  );

  const registrationClosed = useMemo(() => {
    return isRegistrationClosed(
      tournament?.isRegistrationOpen,
      tournament?.registrationEndsDate,
      tournament?.startDate
    );
  }, [
    tournament?.isRegistrationOpen,
    tournament?.registrationEndsDate,
    tournament?.startDate,
  ]);

  async function handleRespondToInvite(
    participantId: string,
    approved: boolean
  ) {
    if (!tournamentId) {
      return;
    }

    setRespondingTo(participantId);

    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved },
      }).unwrap();

      showAlert(
        approved
          ? "Successfully accepted the invite!"
          : "Invite declined.",
        "success"
      );

      refetchInvites();
    } catch (error) {
      console.error(
        "Failed to respond to invite:",
        error
      );
      showAlert(
        "Failed to respond. Please try again.",
        "error"
      );
    } finally {
      setRespondingTo(null);
    }
  }

  async function handleDelete() {
    if (!tournamentId) {
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to delete "${
        tournament?.name ?? "this tournament"
      }"? It will be removed from view.`
    );

    if (!confirmed) {
      return;
    }

    try {
      await deleteTournament({ tournamentId }).unwrap();
      navigate("/tournaments");
    } catch (error) {
      console.error(
        "Failed to delete tournament:",
        error
      );
      showAlert(
        "Failed to delete the tournament. Please try again.",
        "error"
      );
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

  const startDate = new Date(
    tournament.startDate || ""
  );
  const endDate = new Date(
    tournament.endDate || ""
  );

  const formattedDateRange = formatTournamentDateRange(startDate, endDate);

  const handleEdit = () => {
    editModalRef.current?.openEdit({
      id: valueOrDefault(tournament.id, ""),
      name: valueOrDefault(tournament.name, ""),
      description: valueOrDefault(tournament.description, ""),
      startDate: valueOrDefault(tournament.startDate, ""),
      endDate: valueOrDefault(tournament.endDate, ""),
      registrationEndsDate: valueOrDefault(tournament.registrationEndsDate, ""),
      type: valueOrDefault(tournament.type, "" as const),
      country: valueOrDefault(tournament.country, ""),
      city: valueOrDefault(tournament.city, ""),
      place: valueOrDefault(tournament.place, ""),
      organizer: valueOrDefault(tournament.organizer, ""),
      isPrivate: valueOrDefault(tournament.isPrivate, false),
      isRegistrationOpen: valueOrDefault(tournament.isRegistrationOpen, true),
      isVolunteerRegistrationOpen: valueOrDefault(tournament.isVolunteerRegistrationOpen, true),
      bannerImageUrl: valueOrDefault(tournament.bannerImageUrl, ""),
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

  // User action: open the form to register themselves as a volunteer.
  const onOpenVolunteerForm = () => {
    if (!currentUser?.userId) {
      showAlert(
        "You must be signed in to register as a volunteer.",
        "error"
      );
      return;
    }

    volunteerFormModalRef.current?.open(
      tournament.id || "",
      currentUser.userId
    );
  };

  // Manager action: open the volunteer application review modal.
  const onOpenVolunteerReview = () => {
    volunteerReviewModalRef.current?.open(
      tournament.id || "",
      tournament.name || "Unknown Tournament"
    );
  };

  const handleOpenContactOrganizer = () => {
    contactOrganizerModalRef.current?.open({
      name: tournament.organizer || "",
      tournamentName: tournament.name || "",
      tournamentId: tournament.id || "",
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
        isRegistrationClosed={registrationClosed}
        pendingInvitesForUser={pendingInvitesForUser}
        approvedTeamsForUser={approvedTeamsForUser}
        respondingTo={respondingTo}
        rosterSectionRef={rosterSectionRef}
        tournamentId={tournamentId || ""}
        onEdit={handleEdit}
        onOpenRegistrations={() =>
          teamRegistrationsModalRef.current?.open(
            tournament.id || "",
            tournament.name || "Unknown Tournament"
          )
        }
        onOpenVolunteerReview={onOpenVolunteerReview}
        onOpenInviteTeams={() =>
          inviteTeamsModalRef.current?.open(tournament)
        }
        onOpenAddManager={() =>
          setIsAddManagerModalOpen(true)
        }
        onDelete={handleDelete}
        onRespondToInvite={handleRespondToInvite}
        onScrollToRosters={() =>
          rosterSectionRef.current?.scrollIntoView({
            behavior: "smooth",
          })
        }
        onOpenRegister={handleOpenRegister}
        onOpenVolunteerForm={onOpenVolunteerForm}
        onOpenContactOrganizer={handleOpenContactOrganizer}
        onRosterSaved={() => {
          refetchInvites();
          refetchParticipants();
        }}
      />

      <RegisterTournamentModal ref={registerModalRef} />
      <ContactOrganizerModal
        ref={contactOrganizerModalRef}
      />

      <AddTournamentModal ref={editModalRef} />

      <TeamRegistrationsModal
        ref={teamRegistrationsModalRef}
      />

      <VolunteerRegistrationsModal
        ref={volunteerReviewModalRef}
      />

      <VolunteerRegistrationModal
        ref={volunteerFormModalRef}
        teams={participants || []}
        onSaved={() => refetchInvites()}
      />

      <InviteTeamsModal ref={inviteTeamsModalRef} />

      {isAddManagerModalOpen && tournamentId && (
        <AddTournamentManagerModal
          tournamentId={tournamentId}
          onClose={() =>
            setIsAddManagerModalOpen(false)
          }
        />
      )}
    </>
  );
};

export default TournamentDetails;
