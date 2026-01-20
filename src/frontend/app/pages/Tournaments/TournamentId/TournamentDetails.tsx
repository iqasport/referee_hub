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
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

const TournamentDetails = () => {
  const { tournamentId } = useNavigationParams<"tournamentId">();
  const registerModalRef = useRef<RegisterTournamentModalRef>(null);
  const contactOrganizerModalRef = useRef<ContactOrganizerModalRef>(null);
  const editModalRef = useRef<AddTournamentModalRef>(null);
  const registrationsModalRef = useRef<RegistrationsModalRef>(null);
  const inviteTeamsModalRef = useRef<InviteTeamsModalRef>(null);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const { alertState, showAlert, hideAlert } = useAlert();

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

  // Calculate total participant count from all team rosters
  const totalParticipantCount = useMemo(() => {
    if (!participants) return 0;
    return participants.reduce((total, team) => {
      const playerCount = team.players?.length || 0;
      const coachCount = team.coaches?.length || 0;
      const staffCount = team.staff?.length || 0;
      return total + playerCount + coachCount + staffCount;
    }, 0);
  }, [participants]);

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
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
        }}
      >
        <p>Loading tournament...</p>
      </div>
    );
  }

  if (isError || !tournament) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
        }}
      >
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
      type: tournament.type || ("" as const),
      country: tournament.country || "",
      city: tournament.city || "",
      place: tournament.place || "",
      organizer: tournament.organizer || "",
      isPrivate: tournament.isPrivate || false,
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
        isManager={isManager}
      />

      {/* Info cards section */}
      <section style={{ backgroundColor: "#fff", padding: "1.5rem 1rem" }}>
        <div style={{ maxWidth: "72rem", margin: "0 auto", width: "100%" }}>
          <TournamentInfoCards
            formattedDateRange={formattedDateRange}
            organizer={tournament.organizer}
            startDate={tournament.startDate}
            participantCount={totalParticipantCount}
          />

          {/* Main content grid */}
          <div
            style={{
              display: "grid",
              gridTemplateColumns: window.innerWidth >= 1024 ? "2fr 1fr" : "1fr",
              gap: "1rem",
            }}
          >
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
                  <div
                    style={{
                      backgroundColor: "#f0fdf4",
                      borderRadius: "0.5rem",
                      border: "1px solid #bbf7d0",
                      padding: "1.25rem",
                      marginBottom: "1rem",
                      position: window.innerWidth >= 1024 ? "sticky" : "static",
                      top: "1rem",
                    }}
                  >
                    <h3
                      style={{
                        fontSize: "1rem",
                        fontWeight: "bold",
                        color: "#111827",
                        marginBottom: "0.5rem",
                      }}
                    >
                      Manager Tools
                    </h3>
                    <p
                      style={{ fontSize: "0.8125rem", color: "#4b5563", marginBottom: "0.875rem" }}
                    >
                      You are the manager of this tournament. Use the tools below to manage the
                      tournament.
                    </p>
                    <button
                      onClick={handleEdit}
                      style={{
                        width: "100%",
                        backgroundColor: "#16a34a",
                        color: "#fff",
                        fontWeight: "600",
                        padding: "0.5rem 1rem",
                        borderRadius: "0.5rem",
                        transition: "background-color 0.2s",
                        marginBottom: "0.625rem",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        gap: "0.5rem",
                        border: "none",
                        cursor: "pointer",
                        fontSize: "0.875rem",
                      }}
                      onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#15803d")}
                      onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#16a34a")}
                    >
                      <svg
                        style={{ width: "1rem", height: "1rem" }}
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
                      onClick={() => registrationsModalRef.current?.open(tournament.id || "")}
                      style={{
                        width: "100%",
                        backgroundColor: "#fff",
                        border: "1px solid #d1d5db",
                        color: "#374151",
                        fontWeight: "600",
                        padding: "0.5rem 1rem",
                        borderRadius: "0.5rem",
                        transition: "background-color 0.2s",
                        marginBottom: "0.625rem",
                        cursor: "pointer",
                        fontSize: "0.875rem",
                      }}
                      onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#f9fafb")}
                      onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#fff")}
                    >
                      View Team Registrations ({invites?.length || 0})
                    </button>
                    <button
                      onClick={() => inviteTeamsModalRef.current?.open(tournament)}
                      style={{
                        width: "100%",
                        backgroundColor: "#fff",
                        border: "1px solid #d1d5db",
                        color: "#374151",
                        fontWeight: "600",
                        padding: "0.5rem 1rem",
                        borderRadius: "0.5rem",
                        transition: "background-color 0.2s",
                        marginBottom: "0.625rem",
                        cursor: "pointer",
                        fontSize: "0.875rem",
                      }}
                      onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#f9fafb")}
                      onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#fff")}
                    >
                      Invite Teams
                    </button>
                  </div>

                  {/* Tournament Stats Card */}
                  <div
                    style={{
                      backgroundColor: "#fff",
                      borderRadius: "0.5rem",
                      border: "1px solid #e5e7eb",
                      padding: "1.25rem",
                    }}
                  >
                    <h3
                      style={{
                        fontSize: "1rem",
                        fontWeight: "bold",
                        color: "#111827",
                        marginBottom: "0.875rem",
                      }}
                    >
                      Tournament Stats
                    </h3>
                    <div style={{ display: "flex", flexDirection: "column", gap: "0.625rem" }}>
                      <div
                        style={{
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "center",
                          paddingBottom: "0.75rem",
                          borderBottom: "1px solid #e5e7eb",
                        }}
                      >
                        <span style={{ fontSize: "0.875rem", color: "#4b5563" }}>
                          Teams Registered
                        </span>
                        <span style={{ fontSize: "0.875rem", fontWeight: "600", color: "#111827" }}>
                          {invites?.filter((i) => i.status === "approved").length || 0}
                        </span>
                      </div>
                      <div
                        style={{
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "center",
                          paddingBottom: "0.75rem",
                          borderBottom: "1px solid #e5e7eb",
                        }}
                      >
                        <span style={{ fontSize: "0.875rem", color: "#4b5563" }}>
                          Private Tournament
                        </span>
                        <span style={{ fontSize: "0.875rem", fontWeight: "600", color: "#111827" }}>
                          {tournament.isPrivate ? "Yes" : "No"}
                        </span>
                      </div>
                      <div
                        style={{
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "center",
                        }}
                      >
                        <span style={{ fontSize: "0.875rem", color: "#4b5563" }}>
                          Tournament Type
                        </span>
                        <span style={{ fontSize: "0.875rem", fontWeight: "600", color: "#111827" }}>
                          {tournament.type || "N/A"}
                        </span>
                      </div>
                    </div>
                  </div>
                </>
              ) : (
                <>
                  {pendingInvitesForUser.length > 0 && (
                    <div
                      style={{
                        backgroundColor: "#f0fdf4",
                        borderRadius: "0.5rem",
                        border: "1px solid #bbf7d0",
                        padding: "1.25rem",
                        marginBottom: "1rem",
                      }}
                    >
                      <h3
                        style={{
                          fontSize: "1rem",
                          fontWeight: "bold",
                          color: "#111827",
                          marginBottom: "0.5rem",
                        }}
                      >
                        You&apos;re Invited!
                      </h3>
                      <p
                        style={{
                          fontSize: "0.8125rem",
                          color: "#4b5563",
                          marginBottom: "0.875rem",
                        }}
                      >
                        The tournament organizer has invited your team(s) to participate.
                      </p>
                      <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem" }}>
                        {pendingInvitesForUser.map((invite) => (
                          <div
                            key={invite.participantId}
                            style={{
                              backgroundColor: "#fff",
                              borderRadius: "0.5rem",
                              border: "1px solid #e5e7eb",
                              padding: "1rem",
                            }}
                          >
                            <p
                              style={{
                                fontWeight: "600",
                                color: "#111827",
                                marginBottom: "0.75rem",
                              }}
                            >
                              {invite.participantName}
                            </p>
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

                  {/* Register Now Card */}
                  <div
                    style={{
                      backgroundColor: "#fff",
                      borderRadius: "0.5rem",
                      border: "1px solid #e5e7eb",
                      padding: "1.25rem",
                      marginBottom: "1rem",
                      top: "1rem",
                    }}
                  >
                    <h3
                      style={{
                        fontSize: "1rem",
                        fontWeight: "bold",
                        color: "#111827",
                        marginBottom: "0.5rem",
                      }}
                    >
                      Register Now
                    </h3>
                    <p
                      style={{ fontSize: "0.8125rem", color: "#4b5563", marginBottom: "0.875rem" }}
                    >
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
                      style={{
                        width: "100%",
                        backgroundColor: "#16a34a",
                        border: "none",
                        color: "#fff",
                        fontWeight: "600",
                        padding: "0.5rem 1rem",
                        borderRadius: "0.5rem",
                        transition: "background-color 0.2s",
                        cursor: "pointer",
                        fontSize: "0.875rem",
                      }}
                      onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#15803d")}
                      onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#16a34a")}
                    >
                      Register for Tournament
                    </button>
                  </div>

                  {/* Need Help Card */}
                  <div
                    style={{
                      backgroundColor: "#fff",
                      borderRadius: "0.5rem",
                      border: "1px solid #e5e7eb",
                      padding: "1.25rem",
                    }}
                  >
                    <h3
                      style={{
                        fontSize: "1rem",
                        fontWeight: "bold",
                        color: "#111827",
                        marginBottom: "0.5rem",
                      }}
                    >
                      Need Help?
                    </h3>
                    <p
                      style={{ fontSize: "0.8125rem", color: "#4b5563", marginBottom: "0.875rem" }}
                    >
                      Have questions about this tournament? Contact the organizers.
                    </p>
                    <button
                      onClick={() =>
                        contactOrganizerModalRef.current?.open({
                          name: tournament.organizer || "",
                          tournamentName: tournament.name || "",
                        })
                      }
                      style={{
                        width: "100%",
                        backgroundColor: "#16a34a",
                        border: "1px solid #d1d5db",
                        color: "#fff  ",
                        fontWeight: "600",
                        padding: "0.5rem 1rem",
                        borderRadius: "0.5rem",
                        transition: "background-color 0.2s",
                        cursor: "pointer",
                        fontSize: "0.875rem",
                      }}
                      onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#15803d")}
                      onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#16a34a")}
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
            <div style={{ marginTop: "1.5rem" }}>
              <h2
                style={{
                  fontSize: "1.25rem",
                  fontWeight: "bold",
                  color: "#111827",
                  marginBottom: "1rem",
                }}
              >
                Manage Your Team Rosters
              </h2>
              <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
                {approvedTeamsForUser.map((team) => (
                  <RosterManager
                    key={team.teamId}
                    tournamentId={tournamentId || ""}
                    teamId={team.teamId}
                    teamName={team.teamName}
                    ngb={team.ngb}
                    onRosterSaved={() => {
                      refetchInvites();
                      refetchParticipants();
                    }}
                  />
                ))}
              </div>
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
