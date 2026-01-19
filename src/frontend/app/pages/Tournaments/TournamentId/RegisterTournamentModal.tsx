import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useMemo } from "react";
import React from "react";
import {
  useGetCurrentUserQuery,
  useGetManagedTeamsQuery,
  useGetNgbTeamsQuery,
  useCreateInviteMutation,
  useGetTournamentInvitesQuery,
  ManagedTeamViewModel,
  NgbTeamViewModel,
} from "../../../store/serviceApi";

// Role type for current user
interface UserRole {
  roleType?: string;
  teamId?: string;
  teamName?: string;
  ngb?: string | string[];
  team?: string | string[] | { id?: string }; // TeamConstraint from backend
}

interface Tournament {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  country: string;
  city: string;
  type: string;
}

interface ManagedTeam {
  teamId: string;
  teamName: string;
  ngb: string;
  groupAffiliation?: string;
}

interface TeamRegistrationData {
  selectedTeamId: string;
}

export interface RegisterTournamentModalRef {
  open: (tournament: Tournament) => void;
}

const RegisterTournamentModal = forwardRef<RegisterTournamentModalRef>(
  (_props, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    const [tournament, setTournament] = useState<Tournament | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const { data: currentUser } = useGetCurrentUserQuery();

    // Check if user is NGB Admin and get their NGB(s)
    const userNgbs: string[] = useMemo(() => {
      if (!currentUser?.roles) return [];
      const ngbAdminRoles = (currentUser.roles as UserRole[]).filter((r) => r.roleType === "NgbAdmin");
      const ngbs: string[] = [];
      ngbAdminRoles.forEach((r) => {
        if (typeof r.ngb === "string" && r.ngb) {
          ngbs.push(r.ngb);
        } else if (Array.isArray(r.ngb)) {
          ngbs.push(...r.ngb);
        }
      });
      return [...new Set(ngbs)]; // Remove duplicates
    }, [currentUser]);

    const isNgbAdmin = userNgbs.length > 0;
    const primaryNgb = userNgbs[0] || "";

    // Use the new managed teams endpoint - directly fetches teams the user manages
    const { data: managedTeamsData, isLoading: isLoadingManagedTeams } = useGetManagedTeamsQuery();

    // Fetch NGB teams if user is an NGB Admin (for additional teams they can register)
    const { data: ngbTeamsData, isLoading: isLoadingNgbTeams } = useGetNgbTeamsQuery(
      { ngb: primaryNgb, skipPaging: true },
      { skip: !isNgbAdmin || !primaryNgb }
    );

    // Build managed teams from API response and NGB teams (for NGB Admins)
    const managedTeams: ManagedTeam[] = useMemo(() => {
      const teams: ManagedTeam[] = [];
      const addedTeamIds = new Set<string>();
      
      // Add teams from the managedTeams endpoint (team managers)
      if (managedTeamsData) {
        managedTeamsData.forEach((team: ManagedTeamViewModel) => {
          if (team.teamId && !addedTeamIds.has(team.teamId)) {
            addedTeamIds.add(team.teamId);
            teams.push({
              teamId: team.teamId,
              teamName: team.teamName || `Team ${team.teamId}`,
              ngb: team.ngb || "",
            });
          }
        });
      }
      
      // Add NGB teams if user is NGB Admin (avoid duplicates)
      if (isNgbAdmin && ngbTeamsData?.items) {
        ngbTeamsData.items.forEach((team: NgbTeamViewModel) => {
          if (team.teamId && !addedTeamIds.has(team.teamId)) {
            addedTeamIds.add(team.teamId);
            teams.push({
              teamId: team.teamId,
              teamName: team.name || `Team ${team.teamId}`,
              ngb: primaryNgb,
              groupAffiliation: team.groupAffiliation,
            });
          }
        });
      }
      
      return teams;
    }, [managedTeamsData, ngbTeamsData, primaryNgb, isNgbAdmin]);

    // Fetch existing invites for this tournament to check which teams already registered
    const { data: existingInvites } = useGetTournamentInvitesQuery(
      { tournamentId: tournament?.id || "" },
      { skip: !tournament?.id || !isOpen }
    );

    // Get set of team IDs that already have invites (any status)
    const teamsWithExistingInvites = useMemo(() => {
      const teamIds = new Set<string>();
      if (existingInvites) {
        existingInvites.forEach((invite) => {
          if (invite.participantId) {
            teamIds.add(invite.participantId);
          }
        });
      }
      return teamIds;
    }, [existingInvites]);

    // Filter managed teams to only show those without existing invites
    const availableTeams = useMemo(() => {
      return managedTeams.filter((team) => !teamsWithExistingInvites.has(team.teamId));
    }, [managedTeams, teamsWithExistingInvites]);

    // Team registration form data
    const initialTeamData: TeamRegistrationData = {
      selectedTeamId: "",
    };
    const [teamData, setTeamData] = useState<TeamRegistrationData>(initialTeamData);

    // Create invite mutation
    const [createInvite] = useCreateInviteMutation();

    // Loading state for teams
    const isLoadingTeams = isLoadingManagedTeams || (isNgbAdmin && isLoadingNgbTeams);

    useImperativeHandle(ref, () => ({
      open: (tournamentData: Tournament) => {
        setTournament(tournamentData);
        setTeamData(initialTeamData);
        setIsOpen(true);
      },
    }));

    function close() {
      setIsOpen(false);
    }



    // Validation - ensure selected team is in availableTeams (not already registered)
    const isTeamFormValid = !!teamData.selectedTeamId && availableTeams.some(t => t.teamId === teamData.selectedTeamId);

    async function handleSubmit(e: React.FormEvent) {
      e.preventDefault();
      setIsSubmitting(true);

      try {
        // Send team invite to tournament
        await createInvite({
          tournamentId: tournament?.id || "",
          createInviteModel: {
            participantType: "team",
            participantId: teamData.selectedTeamId,
          },
        }).unwrap();

        const selectedTeam = availableTeams.find(t => t.teamId === teamData.selectedTeamId);
        console.log("Team invite sent:", {
          tournamentId: tournament?.id,
          teamId: teamData.selectedTeamId,
          teamName: selectedTeam?.teamName,
        });

        alert(`Successfully sent invite for ${selectedTeam?.teamName} to ${tournament?.name}! The tournament organizer will review your request.`);

        close();
      } catch (error: unknown) {
        console.error("Failed to register for tournament:", error);
        const errorMessage = error instanceof Error ? error.message : "Failed to register for tournament. Please try again.";
        alert(errorMessage);
      } finally {
        setIsSubmitting(false);
      }
    }

    const startDate = tournament ? new Date(tournament.startDate) : null;
    const endDate = tournament ? new Date(tournament.endDate) : null;
    const formattedDateRange =
      startDate && endDate
        ? `${startDate.toLocaleDateString("en-US", {
            month: "short",
            day: "numeric",
          })}=${endDate.toLocaleDateString("en-US", {
            day: "numeric",
            year: "numeric",
          })}`
        : "";

    return (
      <Dialog
        open={isOpen && !!tournament}
        as="div"
        className="relative z-50"
        onClose={close}
      >
        <div className="fixed inset-0" style={{ backgroundColor: 'rgba(0, 0, 0, 0.3)' }} aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
          <DialogPanel className="relative w-full max-w-2xl rounded bg-white p-6 shadow-xl my-8 overflow-y-auto" style={{ maxHeight: '90vh' }}>
            <DialogTitle
              as="h3"
              className="text-xl font-semibold text-gray-900 mb-1"
            >
              Register for Tournament
            </DialogTitle>

            {/* Tournament Info */}
            <p className="text-sm text-gray-600 mb-4">
              {tournament?.name} - {formattedDateRange} - {tournament?.city}
            </p>

            <form onSubmit={handleSubmit} className="space-y-4">
              <>
                  {/* Team Selection */}
                  <div>
                    <h5 className="text-sm font-semibold text-gray-900 mb-3">
                      Team Information
                    </h5>
                    <div className="mb-4">
                      <label
                        htmlFor="teamSelect"
                        className="block text-sm font-medium text-gray-700 mb-1"
                      >
                        Select an existing team you manage
                      </label>
                      {isLoadingTeams ? (
                        <div className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-gray-500">
                          Loading teams...
                        </div>
                      ) : (
                        <select
                          id="teamSelect"
                          value={teamData.selectedTeamId}
                          onChange={(e) =>
                            setTeamData((prev) => ({
                              ...prev,
                              selectedTeamId: e.target.value,
                            }))
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500 bg-white"
                        >
                          <option value="">Select an existing team you manage</option>
                          {availableTeams.map((team) => (
                            <option key={team.teamId} value={team.teamId}>
                              {team.teamName}
                            </option>
                          ))}
                        </select>
                      )}
                      {!isLoadingTeams && managedTeams.length === 0 && (
                        <p className="text-sm text-amber-600 mt-2">
                          No teams found. You must be an NGB Admin with registered teams or a Team Manager to register for a tournament.
                        </p>
                      )}
                      {!isLoadingTeams && managedTeams.length > 0 && availableTeams.length === 0 && (
                        <p className="text-sm text-amber-600 mt-2">
                          All your teams have already registered for this tournament.
                        </p>
                      )}
                      {/* Show already registered teams */}
                      {teamsWithExistingInvites.size > 0 && managedTeams.some(t => teamsWithExistingInvites.has(t.teamId)) && (
                        <div className="mt-3 p-3 rounded" style={{ backgroundColor: "#f3f4f6" }}>
                          <p className="text-xs text-gray-600 mb-2">Teams already registered:</p>
                          <div className="text-sm text-gray-700">
                            {managedTeams
                              .filter(t => teamsWithExistingInvites.has(t.teamId))
                              .map(t => {
                                const invite = existingInvites?.find(i => i.participantId === t.teamId);
                                const status = invite?.status || "unknown";
                                return (
                                  <div key={t.teamId} className="flex items-center justify-between py-1">
                                    <span>{t.teamName}</span>
                                    <span
                                      className="px-2 py-0.5 rounded-full text-xs font-semibold"
                                      style={{
                                        backgroundColor: status === "pending" ? "#fef3c7" : status === "approved" ? "#d1fae5" : "#fee2e2",
                                        color: status === "pending" ? "#92400e" : status === "approved" ? "#065f46" : "#991b1b"
                                      }}
                                    >
                                      {status.charAt(0).toUpperCase() + status.slice(1)}
                                    </span>
                                  </div>
                                );
                              })}
                          </div>
                        </div>
                      )}
                    </div>
                  </div>





                  {/* Info Note */}
                  <p className="text-xs text-gray-500">
                    Once the tournament organizer approves your invite, you&apos;ll be able to manage your team&apos;s roster and submit your player list.
              </p>
              </>

              {/* Action Buttons */}
              <div className="flex justify-end mt-6 pt-4 border-t border-gray-200">
                <button
                  type="button"
                  onClick={close}
                  className="px-6 py-2 text-sm font-medium rounded mr-3"
                  style={{
                    backgroundColor: "#edf2f7",
                    color: "#4a5568",
                    border: "1px solid #e2e8f0"
                  }}
                >
                  Cancel
                </button>
              <button
                type="submit"
                disabled={isSubmitting || !isTeamFormValid}
                className="px-6 py-2 text-sm font-medium rounded"
                style={{
                  backgroundColor: (isSubmitting || !isTeamFormValid) ? "#90cdf4" : "#3182ce",
                  color: "#ffffff",
                  border: "1px solid #3182ce",
                  cursor: (isSubmitting || !isTeamFormValid) ? "not-allowed" : "pointer"
                }}
              >
                {isSubmitting ? "Submitting..." : "Submit Registration"}
              </button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </Dialog>
    );
  }
);

RegisterTournamentModal.displayName = "RegisterTournamentModal";

export default RegisterTournamentModal;
