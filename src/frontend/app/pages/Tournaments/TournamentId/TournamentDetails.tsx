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
