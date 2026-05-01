import React, { useRef, useState } from "react";
import RegisterTournamentModal, { RegisterTournamentModalRef } from "./RegisterTournamentModal";
import ContactOrganizerModal, { ContactOrganizerModalRef } from "./ContactOrganizerModal";
import AddTournamentModal, { AddTournamentModalRef } from "../components/AddTournamentModal";
import RegistrationsModal, { RegistrationsModalRef } from "./RegistrationsModal";
import InviteTeamsModal, { InviteTeamsModalRef } from "./InviteTeamsModal";
import AddTournamentManagerModal from "./AddTournamentManagerModal";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import {
  TournamentHeader,
  TournamentInfoCards,
  TournamentAboutSection,
  RosterManager,
} from "./components";
import { TournamentDetailsSidebar } from "./components/TournamentDetailsSidebar";
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
                  registrationsModalRef.current?.open(tournament.id || "", tournament.name || "Unknown Tournament")
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
