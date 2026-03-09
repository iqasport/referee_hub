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
  useDeleteTournamentMutation,
  TournamentInviteViewModel,
  TournamentViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams, useNavigate } from "../../../utils/navigationUtils";
import { getApiErrorMessage } from "../../../utils/tournamentUtils";

// ── Helpers ──────────────────────────────────────────────────────────────────

function buildTournamentEditPayload(tournament: TournamentViewModel) {
  const {
    id = "", name = "", description = "", startDate = "",
    endDate = "", registrationEndsDate = "", type = "",
    country = "", city = "", place = "", organizer = "",
    isPrivate = false, isRegistrationOpen = true,
    bannerImageUrl = "",
  } = tournament;
  return {
    id, name, description, startDate, endDate, registrationEndsDate,
    type: type as "" | import("../../../store/serviceApi").TournamentType,
    country, city, place, organizer, isPrivate, isRegistrationOpen,
    bannerImageUrl,
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
  onScrollToRoster: () => void;
  onOpenRegisterModal: () => void;
}
const UserRegistrationCard: React.FC<UserRegistrationCardProps> = ({
  isRegistrationClosed, approvedTeams, onScrollToRoster, onOpenRegisterModal,
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

// ── AddManagerForm ────────────────────────────────────────────────────────────
interface AddManagerFormProps {
  email: string;
  isLoading: boolean;
  onChange: (v: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  onCancel: () => void;
}
const AddManagerForm: React.FC<AddManagerFormProps> = ({ email, isLoading, onChange, onSubmit, onCancel }) => (
  <div className="card card-mb">
    <h3 className="card-title">Add Tournament Manager</h3>
    <p className="card-description">Enter the email address of the user you want to add as a tournament manager.</p>
    <form onSubmit={onSubmit} className="space-y-3">
      <input
        type="email"
        value={email}
        onChange={(e) => onChange(e.target.value)}
        placeholder="Email address"
        required
        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
      />
      <div className="flex gap-2">
        <button type="submit" disabled={isLoading || !email.trim()} className="btn btn-primary">
          {isLoading ? "Adding..." : "Add Manager"}
        </button>
        <button type="button" onClick={onCancel} className="btn btn-secondary">Cancel</button>
      </div>
    </form>
  </div>
);

// ── TournamentStats ───────────────────────────────────────────────────────────
interface TournamentStatsProps {
  approvedCount: number;
  playerCount: number;
  isPrivate: boolean | null | undefined;
}
const TournamentStats: React.FC<TournamentStatsProps> = ({ approvedCount, playerCount, isPrivate }) => (
  <div className="card card-mb">
    <h3 className="card-title">Tournament Stats</h3>
    <div className="stats-list">
      <div className="stats-item">
        <span className="stats-label">Teams Registered</span>
        <span className="stats-value">{approvedCount}</span>
      </div>
      <div className="stats-item">
        <span className="stats-label">Players Registered</span>
        <span className="stats-value">{playerCount}</span>
      </div>
      <div className="stats-item">
        <span className="stats-label">Private Tournament</span>
        <span className="stats-value">{isPrivate ? "Yes" : "No"}</span>
      </div>
    </div>
  </div>
);

// ── ManagerSidebar ────────────────────────────────────────────────────────────
interface ManagerSidebarProps {
  tournament: TournamentViewModel;
  inviteCount: number;
  approvedCount: number;
  playerCount: number;
  isAddManagerOpen: boolean;
  addManagerEmail: string;
  isAddingManager: boolean;
  pendingInvitesForUser: TournamentInviteViewModel[];
  respondingTo: string | null;
  approvedTeamsForUser: ApprovedTeam[];
  isRegistrationClosed: boolean;
  onEdit: () => void;
  onDelete: () => void;
  onViewRegistrations: () => void;
  onInviteTeams: () => void;
  onAddManagerOpen: () => void;
  onAddManagerEmailChange: (v: string) => void;
  onAddManagerSubmit: (e: React.FormEvent) => void;
  onAddManagerCancel: () => void;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
  onScrollToRoster: () => void;
  onOpenRegisterModal: () => void;
  managedTeamsCount: number;
}
const ManagerSidebar: React.FC<ManagerSidebarProps> = ({
  tournament, inviteCount, approvedCount, playerCount,
  isAddManagerOpen, addManagerEmail, isAddingManager,
  pendingInvitesForUser, respondingTo, approvedTeamsForUser,
  isRegistrationClosed, onEdit, onDelete, onViewRegistrations, onInviteTeams,
  onAddManagerOpen, onAddManagerEmailChange, onAddManagerSubmit, onAddManagerCancel,
  onAccept, onDecline, onScrollToRoster, onOpenRegisterModal, managedTeamsCount,
}) => (
  <>
    <div className="card card-highlighted card-mb card-sticky">
      <h3 className="card-title">Manager Tools</h3>
      <p className="card-description">
        You are the manager of this tournament. Use the tools below to manage the tournament.
      </p>
      <button onClick={onEdit} className="btn btn-primary btn-full-width btn-with-icon card-mb">
        <svg className="btn-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
            d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
        </svg>
        Edit Tournament Details
      </button>
      <button onClick={onViewRegistrations} className="btn btn-secondary btn-full-width card-mb">
        View Team Registrations ({inviteCount})
      </button>
      <button onClick={onInviteTeams} className="btn btn-secondary btn-full-width card-mb">
        Invite Teams
      </button>
      <button onClick={onAddManagerOpen} className="btn btn-secondary btn-full-width">
        Add Tournament Manager
      </button>
      <button onClick={onDelete} className="btn btn-danger btn-full-width" style={{ marginTop: "0.75rem" }}>
        Delete Tournament
      </button>
    </div>

    {isAddManagerOpen && (
      <AddManagerForm
        email={addManagerEmail}
        isLoading={isAddingManager}
        onChange={onAddManagerEmailChange}
        onSubmit={onAddManagerSubmit}
        onCancel={onAddManagerCancel}
      />
    )}

    <TournamentStats
      approvedCount={approvedCount}
      playerCount={playerCount}
      isPrivate={tournament.isPrivate}
    />

    {pendingInvitesForUser.length > 0 && (
      <PendingInvitesCard
        pendingInvites={pendingInvitesForUser}
        respondingTo={respondingTo}
        isManager={true}
        onAccept={onAccept}
        onDecline={onDecline}
      />
    )}

    {managedTeamsCount > 0 && (
      <ManagerTeamCard
        isRegistrationClosed={isRegistrationClosed}
        approvedTeams={approvedTeamsForUser}
        onScrollToRoster={onScrollToRoster}
        onOpenRegisterModal={onOpenRegisterModal}
      />
    )}
  </>
);

// ── UserSidebar ───────────────────────────────────────────────────────────────
interface UserSidebarProps {
  pendingInvitesForUser: TournamentInviteViewModel[];
  respondingTo: string | null;
  isRegistrationClosed: boolean;
  approvedTeamsForUser: ApprovedTeam[];
  tournament: TournamentViewModel;
  contactOrganizerRef: React.RefObject<{ open: (args: { name: string; tournamentName: string; tournamentId?: string | null }) => void }>;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
  onScrollToRoster: () => void;
  onOpenRegisterModal: () => void;
  approvedCount: number;
  playerCount: number;
}
const UserSidebar: React.FC<UserSidebarProps> = ({
  pendingInvitesForUser, respondingTo, isRegistrationClosed,
  approvedTeamsForUser, tournament, contactOrganizerRef,
  onAccept, onDecline, onScrollToRoster, onOpenRegisterModal,
  approvedCount, playerCount,
}) => (
  <>
    <TournamentStats
      approvedCount={approvedCount}
      playerCount={playerCount}
      isPrivate={tournament.isPrivate ?? false}
    />
    {pendingInvitesForUser.length > 0 && (
      <PendingInvitesCard
        pendingInvites={pendingInvitesForUser}
        respondingTo={respondingTo}
        isManager={false}
        onAccept={onAccept}
        onDecline={onDecline}
      />
    )}
    <UserRegistrationCard
      isRegistrationClosed={isRegistrationClosed}
      approvedTeams={approvedTeamsForUser}
      onScrollToRoster={onScrollToRoster}
      onOpenRegisterModal={onOpenRegisterModal}
    />
    <div className="card">
      <h3 className="card-title">Need Help?</h3>
      <p className="card-description">Have questions about this tournament? Contact the organizers.</p>
      <button
        onClick={() => contactOrganizerRef.current?.open({
          name: tournament.organizer ?? "",
          tournamentName: tournament.name ?? "",
          tournamentId: tournament.id,
        })}
        className="btn btn-outline btn-full-width"
      >
        Contact Organizer
      </button>
    </div>
  </>
);

// ── Data hook ─────────────────────────────────────────────────────────────────

function useTournamentDetailsData(tournamentId: string | undefined) {
  const { data: tournament, isLoading, isError, error: tournamentError } = useGetTournamentQuery(
    { tournamentId: tournamentId ?? "" },
  );
  // 401 → private tournament that requires sign-in; distinguish from 404 (truly not found)
  const tournamentRequiresAuth = !isLoading && (tournamentError as any)?.status === 401;
  const { data: currentUser } = useGetCurrentUserQuery();
  const { data: managedTeamsData } = useGetManagedTeamsQuery(undefined, { skip: !currentUser });

  const isTournamentManagerOfThis = useMemo(
    () => currentUser?.roles?.some((role: any) => {
      if (role.roleType !== "TournamentManager") return false;
      if (role.tournament === "ANY") return true;
      if (Array.isArray(role.tournament)) return role.tournament.includes(tournamentId);
      return role.tournament === tournamentId;
    }),
    [currentUser?.roles, tournamentId],
  );

  const shouldFetchManagers = Boolean(tournamentId && isTournamentManagerOfThis);
  const { data: managers, isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId ?? "" },
    { skip: !shouldFetchManagers },
  );

  const { data: invites, refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId ?? "" },
    { skip: !tournamentId || isError },
  );
  const { data: participants, refetch: refetchParticipants } = useGetParticipantsQuery(
    { tournamentId: tournamentId ?? "" },
    { skip: !tournamentId || isError },
  );

  const managedTeamIds = useMemo(() => {
    const ids = new Set<string>();
    managedTeamsData?.forEach((t) => { if (t.teamId) ids.add(t.teamId); });
    return ids;
  }, [managedTeamsData]);

  const pendingInvitesForUser: TournamentInviteViewModel[] = useMemo(() => {
    if (!invites || managedTeamIds.size === 0) return [];
    return invites.filter((i) =>
      i.participantId && managedTeamIds.has(i.participantId) && i.participantApproval?.status === "pending",
    );
  }, [invites, managedTeamIds]);

  const approvedTeamsForUser = useMemo(() => {
    if (!invites || managedTeamIds.size === 0 || !managedTeamsData) return [];
    return invites
      .filter((i) => i.participantId && managedTeamIds.has(i.participantId) && i.status === "approved")
      .map((i) => {
        const td = managedTeamsData.find((t) => t.teamId === i.participantId);
        return { teamId: i.participantId, teamName: i.participantName ?? td?.teamName ?? "Unknown Team", ngb: td?.ngb ?? "" };
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

  return {
    tournament, isLoading, isError, tournamentRequiresAuth,
    currentUser, managers, managersError,
    invites, refetchInvites,
    refetchParticipants,
    managedTeamsData,
    pendingInvitesForUser, approvedTeamsForUser,
    totalPlayerCount, isRegistrationClosed,
  };
}

// ── Actions hook ──────────────────────────────────────────────────────────────

function useTournamentActions(
  tournamentId: string | undefined,
  tournamentName: string | undefined,
  showAlert: (msg: string, type: "success" | "error") => void,
  refetchInvites: () => void,
) {
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [addManagerEmail, setAddManagerEmail] = useState("");
  const [isAddManagerOpen, setIsAddManagerOpen] = useState(false);
  const [addTournamentManager, { isLoading: isAddingManager }] = useAddTournamentManagerMutation();
  const [deleteTournament] = useDeleteTournamentMutation();
  const navigate = useNavigate();
  const [respondToInvite] = useRespondToInviteMutation();

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

  async function handleDelete() {
    if (!tournamentId) return;
    if (!window.confirm(`Are you sure you want to delete "${tournamentName ?? "this tournament"}"? This action cannot be undone.`)) return;
    try {
      await deleteTournament({ tournamentId }).unwrap();
      navigate("/tournaments");
    } catch (error) {
      console.error("Failed to delete tournament:", error);
      showAlert(getApiErrorMessage(error, "Failed to delete the tournament. Please try again."), "error");
    }
  }

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

  return {
    respondingTo, addManagerEmail, setAddManagerEmail,
    isAddManagerOpen, setIsAddManagerOpen, isAddingManager,
    handleAddManager,
    handleCancelAddManager: () => { setIsAddManagerOpen(false); setAddManagerEmail(""); },
    handleDelete, handleRespondToInvite,
  };
}

function formatDateRange(startDateStr?: string | null, endDateStr?: string | null): string {
  const startDate = new Date(startDateStr || "");
  const endDate = new Date(endDateStr || "");
  const isSameDay = startDate.toDateString() === endDate.toDateString();
  return isSameDay
    ? startDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })
    : `${startDate.toLocaleDateString("en-US", { month: "short", day: "numeric" })} - ${endDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}`;
}

// ── Main component ─────────────────────────────────────────────────────────────

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef = useRef<ContactOrganizerModalRef>(null);
  const editModalRef = useRef<AddTournamentModalRef>(null);
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);
  const rosterSectionRef = useRef<HTMLDivElement>(null);
  const { alertState, showAlert, hideAlert } = useAlert();

  const {
    tournament, isLoading, isError, tournamentRequiresAuth,
    currentUser, managers, managersError,
    invites, refetchInvites, refetchParticipants,
    managedTeamsData,
    pendingInvitesForUser, approvedTeamsForUser,
    totalPlayerCount, isRegistrationClosed,
  } = useTournamentDetailsData(tournamentId);

  const {
    respondingTo, addManagerEmail, setAddManagerEmail,
    isAddManagerOpen, setIsAddManagerOpen, isAddingManager,
    handleAddManager, handleCancelAddManager, handleDelete, handleRespondToInvite,
  } = useTournamentActions(tournamentId, tournament?.name, showAlert, refetchInvites);

  if (isLoading) {
    return (
      <div className="tournament-details-loading">
        <p>Loading tournament...</p>
      </div>
    );
  }

  if (tournamentRequiresAuth) {
    return (
      <div className="tournament-details-error">
        <p>This is a private tournament. Please <a href="/sign_in" className="underline text-blue-600">sign in</a> to view it.</p>
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

  const handleEdit = () => editModalRef.current?.openEdit(buildTournamentEditPayload(tournament));
  const openRegisterModal = () => registerModalRef.current?.open(buildRegisterModalPayload(tournament));
  const scrollToRoster = () => rosterSectionRef.current?.scrollIntoView({ behavior: "smooth" });
  const approvedInviteCount = invites?.filter((i) => i.status === "approved").length ?? 0;

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
              {!currentUser ? (
                <>
                  <TournamentStats
                    approvedCount={approvedInviteCount}
                    playerCount={totalPlayerCount}
                    isPrivate={tournament.isPrivate}
                  />
                  <div className="card">
                    <p className="text-center" style={{ padding: "1.5rem", color: "#555" }}>
                      <a href="/sign_in" className="text-link">Sign in</a> to register for this tournament or contact the organizer.
                    </p>
                  </div>
                </>
              ) : isManager ? (
                <ManagerSidebar
                  tournament={tournament}
                  inviteCount={invites?.length ?? 0}
                  approvedCount={approvedInviteCount}
                  playerCount={totalPlayerCount}
                  isAddManagerOpen={isAddManagerOpen}
                  addManagerEmail={addManagerEmail}
                  isAddingManager={isAddingManager}
                  pendingInvitesForUser={pendingInvitesForUser}
                  respondingTo={respondingTo}
                  approvedTeamsForUser={approvedTeamsForUser}
                  isRegistrationClosed={isRegistrationClosed}
                  onEdit={handleEdit}
                  onDelete={handleDelete}
                  onViewRegistrations={() => registrationsModalRef.current?.open(tournament.id ?? "", tournament.name ?? "Unknown Tournament")}
                  onInviteTeams={() => inviteTeamsModalRef.current?.open(tournament)}
                  onAddManagerOpen={() => setIsAddManagerOpen(true)}
                  onAddManagerEmailChange={setAddManagerEmail}
                  onAddManagerSubmit={handleAddManager}
                  onAddManagerCancel={handleCancelAddManager}
                  onAccept={(id) => handleRespondToInvite(id, true)}
                  onDecline={(id) => handleRespondToInvite(id, false)}
                  onScrollToRoster={scrollToRoster}
                  onOpenRegisterModal={openRegisterModal}
                  managedTeamsCount={managedTeamsData?.length ?? 0}
                />
              ) : (
                <UserSidebar
                  pendingInvitesForUser={pendingInvitesForUser}
                  respondingTo={respondingTo}
                  isRegistrationClosed={isRegistrationClosed}
                  approvedTeamsForUser={approvedTeamsForUser}
                  tournament={tournament}
                  contactOrganizerRef={contactOrganizerModalRef as React.RefObject<{ open: (args: { name: string; tournamentName: string; tournamentId?: string | null }) => void }>}
                  onAccept={(id) => handleRespondToInvite(id, true)}
                  onDecline={(id) => handleRespondToInvite(id, false)}
                  onScrollToRoster={scrollToRoster}
                  onOpenRegisterModal={openRegisterModal}
                  approvedCount={approvedInviteCount}
                  playerCount={totalPlayerCount}
                />
              )}
            </div>
          </div>

          {approvedTeamsForUser.length > 0 && (
            <div ref={rosterSectionRef} className="roster-section">
              <h2 className="card-title card-title-lg">Manage Your Team Rosters</h2>
              <RosterManager
                tournamentId={tournamentId ?? ""}
                teams={approvedTeamsForUser}
                onRosterSaved={() => { refetchInvites(); refetchParticipants(); }}
              />
            </div>
          )}
        </div>
      </section>

      <RegisterTournamentModal ref={registerModalRef} />
      <ContactOrganizerModal ref={contactOrganizerModalRef} />
      <AddTournamentModal ref={editModalRef} />
      <RegistrationsModal ref={registrationsModalRef} />
      <InviteTeamsModal ref={inviteTeamsModalRef} />
    </>
  );
};

export default TournamentDetails;
