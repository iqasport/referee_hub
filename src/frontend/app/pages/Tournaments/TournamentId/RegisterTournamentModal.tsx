import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useEffect, useMemo } from "react";
import React from "react";
import {
  useGetCurrentUserQuery,
  useGetTeamMembersQuery,
  useGetNgbTeamsQuery,
  useCreateInviteMutation,
  NgbTeamViewModel,
  TeamMemberViewModel,
} from "../../../store/serviceApi";

// Role type for current user
interface UserRole {
  roleType?: string;
  teamId?: string;
  teamName?: string;
  ngb?: string | string[];
}

interface Tournament {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  country: string;
  city: string;
}

type RegistrationMode = "team" | "player";

interface ManagedTeam {
  teamId: string;
  teamName: string;
  ngb: string;
}

interface TeamMember {
  userId: string;
  name: string;
}

interface SelectedPlayer {
  userId: string;
  notes: string;
}

interface TeamRegistrationData {
  selectedTeamId: string;
  selectedPlayers: SelectedPlayer[];
  additionalNotes: string;
}

interface PlayerRegistrationData {
  selectedTeamId: string; 
  contactName: string;
  contactEmail: string;
  contactPhone: string;
  additionalNotes: string;
}

export interface RegisterTournamentModalRef {
  open: (tournament: Tournament) => void;
}

const RegisterTournamentModal = forwardRef<RegisterTournamentModalRef>(
  (_props, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    const [tournament, setTournament] = useState<Tournament | null>(null);
    const [mode, setMode] = useState<RegistrationMode>("team");
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

    // Fetch NGB teams if user is an NGB Admin
    const { data: ngbTeamsData, isLoading: isLoadingNgbTeams } = useGetNgbTeamsQuery(
      { ngb: primaryNgb, skipPaging: true },
      { skip: !isNgbAdmin || !primaryNgb }
    );

    // Extract managed teams from TeamManager roles
    const teamManagerTeams: ManagedTeam[] = useMemo(() => {
      if (!currentUser?.roles) return [];
      return (currentUser.roles as UserRole[])
        .filter((r) => r.roleType === "TeamManager")
        .map((r) => ({
          teamId: r.teamId || "",
          teamName: r.teamName || `Team ${r.teamId}`,
          ngb: (typeof r.ngb === "string" ? r.ngb : "") || "",
        }));
    }, [currentUser]);

    // Combine NGB teams (for NGB Admins) with TeamManager teams
    const managedTeams: ManagedTeam[] = useMemo(() => {
      const teams: ManagedTeam[] = [...teamManagerTeams];
      
      // Add NGB teams if user is NGB Admin
      if (isNgbAdmin && ngbTeamsData?.items) {
        ngbTeamsData.items.forEach((team: NgbTeamViewModel) => {
          // Avoid duplicates
          if (!teams.some(t => t.teamId === team.teamId)) {
            teams.push({
              teamId: team.teamId || "",
              teamName: team.name || `Team ${team.teamId}`,
              ngb: primaryNgb,
            });
          }
        });
      }
      
      return teams;
    }, [teamManagerTeams, isNgbAdmin, ngbTeamsData, primaryNgb]);

    // Extract teams the player belongs to (for player registration)
    const playerTeams: ManagedTeam[] = useMemo(() => {
      // For player registration, show all available teams (same as managedTeams for now)
      return managedTeams;
    }, [managedTeams]);

    // Team registration form data
    const initialTeamData: TeamRegistrationData = {
      selectedTeamId: "",
      selectedPlayers: [],
      additionalNotes: "",
    };
    const [teamData, setTeamData] = useState<TeamRegistrationData>(initialTeamData);

    // Player registration form data
    const initialPlayerData: PlayerRegistrationData = {
      selectedTeamId: "",
      contactName: "",
      contactEmail: "",
      contactPhone: "",
      additionalNotes: "",
    };
    const [playerData, setPlayerData] = useState<PlayerRegistrationData>(initialPlayerData);

    // Get the selected team's ngb for fetching members
    const selectedManagedTeam = managedTeams.find(t => t.teamId === teamData.selectedTeamId);

    // Fetch team members when a team is selected (team manager flow)
    const { data: teamMembersData, isLoading: isLoadingMembers } = useGetTeamMembersQuery(
      {
        ngb: selectedManagedTeam?.ngb || "",
        teamId: teamData.selectedTeamId,
        skipPaging: true,
      },
      {
        skip: !teamData.selectedTeamId || !selectedManagedTeam?.ngb,
      }
    );

    const teamMembers: TeamMember[] = useMemo(() => {
      if (!teamMembersData?.items) return [];
      return teamMembersData.items.map((m: TeamMemberViewModel) => ({
        userId: m.userId || "",
        name: m.name || "Unknown",
      }));
    }, [teamMembersData]);

    // Create invite mutation
    const [createInvite] = useCreateInviteMutation();

    // Loading state for teams
    const isLoadingTeams = isNgbAdmin && isLoadingNgbTeams;

    // Pre-fill player data with current user info
    useEffect(() => {
      if (currentUser && isOpen) {
        setPlayerData(prev => ({
          ...prev,
          contactName: `${currentUser.firstName || ""} ${currentUser.lastName || ""}`.trim(),
        }));
      }
    }, [currentUser, isOpen]);

    useImperativeHandle(ref, () => ({
      open: (tournamentData: Tournament) => {
        setTournament(tournamentData);
        setMode("team");
        setTeamData(initialTeamData);
        setPlayerData({
          ...initialPlayerData,
          contactName: currentUser 
            ? `${currentUser.firstName || ""} ${currentUser.lastName || ""}`.trim()
            : "",
        });
        setIsOpen(true);
      },
    }));

    function close() {
      setIsOpen(false);
    }

    // Toggle player selection
    function togglePlayerSelection(userId: string) {
      setTeamData(prev => {
        const existingPlayer = prev.selectedPlayers.find(p => p.userId === userId);
        if (existingPlayer) {
          // Remove player
          return {
            ...prev,
            selectedPlayers: prev.selectedPlayers.filter(p => p.userId !== userId),
          };
        } else {
          // Add player with empty notes
          return {
            ...prev,
            selectedPlayers: [...prev.selectedPlayers, { userId, notes: "" }],
          };
        }
      });
    }

    // Update player notes
    function updatePlayerNotes(userId: string, notes: string) {
      setTeamData(prev => ({
        ...prev,
        selectedPlayers: prev.selectedPlayers.map(p =>
          p.userId === userId ? { ...p, notes } : p
        ),
      }));
    }

    // Check if player is selected
    function isPlayerSelected(userId: string): boolean {
      return teamData.selectedPlayers.some(p => p.userId === userId);
    }

    // Get player notes
    function getPlayerNotes(userId: string): string {
      return teamData.selectedPlayers.find(p => p.userId === userId)?.notes || "";
    }

    // Select/deselect all players
    function selectAllPlayers() {
      setTeamData(prev => ({
        ...prev,
        selectedPlayers: teamMembers.map(m => {
          // Preserve existing notes if player was already selected
          const existing = prev.selectedPlayers.find(p => p.userId === m.userId);
          return { userId: m.userId, notes: existing?.notes || "" };
        }),
      }));
    }

    function deselectAllPlayers() {
      setTeamData(prev => ({
        ...prev,
        selectedPlayers: [],
      }));
    }

    // Validation
    const teamValidationErrors: string[] = useMemo(() => {
      const errors: string[] = [];
      if (!teamData.selectedTeamId) {
        errors.push("Please select a team");
      }
      if (teamData.selectedPlayers.length === 0 && teamData.selectedTeamId) {
        errors.push("Please select at least one player");
      }
      return errors;
    }, [teamData]);

    const playerValidationErrors: string[] = useMemo(() => {
      const errors: string[] = [];
      if (!playerData.contactName.trim()) {
        errors.push("Contact name is required");
      }
      if (!playerData.contactEmail.trim()) {
        errors.push("Email is required");
      }
      if (!playerData.contactPhone.trim()) {
        errors.push("Phone number is required");
      }
      return errors;
    }, [playerData]);

    const isTeamFormValid = teamValidationErrors.length === 0 && teamData.selectedTeamId;
    const isPlayerFormValid = playerValidationErrors.length === 0;

    async function handleSubmit(e: React.FormEvent) {
      e.preventDefault();
      setIsSubmitting(true);

      try {
        if (mode === "team") {
          // Team registration: Create invite for the team
          await createInvite({
            tournamentId: tournament?.id || "",
            createInviteModel: {
              participantType: "team",
              participantId: teamData.selectedTeamId,
            },
          }).unwrap();

          // TODO: When roster API is available, update the roster with selected players and their notes
          // await updateRoster({
          //   tournamentId: tournament?.id,
          //   teamId: teamData.selectedTeamId,
          //   players: teamData.selectedPlayers,
          //   additionalNotes: teamData.additionalNotes,
          // }).unwrap();

          console.log("Team registration submitted:", {
            tournamentId: tournament?.id,
            teamId: teamData.selectedTeamId,
            selectedPlayers: teamData.selectedPlayers,
            additionalNotes: teamData.additionalNotes,
          });

          alert(`Successfully registered team for ${tournament?.name}! The organizer will review your registration.`);
        } else {
          // Player registration
          // TODO: Implement player registration API when available
          console.log("Player registration submitted:", {
            tournamentId: tournament?.id,
            teamId: playerData.selectedTeamId || null,
            contactName: playerData.contactName,
            contactEmail: playerData.contactEmail,
            contactPhone: playerData.contactPhone,
            notes: playerData.additionalNotes,
          });

          alert(`Successfully registered as a player for ${tournament?.name}! The organizer or team manager will review your registration.`);
        }

        close();
      } catch (error) {
        console.error("Failed to register for tournament:", error);
        alert("Failed to register for tournament. Please try again.");
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

            {/* Mode Toggle Buttons */}
            <div className="flex mb-6">
              <button
                type="button"
                onClick={() => setMode("team")}
                className="flex-1 py-2 px-4 rounded font-medium mr-2"
                style={{
                  backgroundColor: mode === "team" ? "#3182ce" : "#edf2f7",
                  color: mode === "team" ? "#ffffff" : "#4a5568",
                  border: "1px solid #e2e8f0"
                }}
              >
                Register Team
              </button>
              <button
                type="button"
                onClick={() => setMode("player")}
                className="flex-1 py-2 px-4 rounded font-medium"
                style={{
                  backgroundColor: mode === "player" ? "#3182ce" : "#edf2f7",
                  color: mode === "player" ? "#ffffff" : "#4a5568",
                  border: "1px solid #e2e8f0"
                }}
              >
                Register Player
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              {mode === "team" && (
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
                              selectedPlayerIds: [], // Reset player selection when team changes
                            }))
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500 bg-white"
                        >
                          <option value="">Select an existing team you manage</option>
                          {managedTeams.map((team) => (
                            <option key={team.teamId} value={team.teamId}>
                              {team.teamName}
                            </option>
                          ))}
                        </select>
                      )}
                      {!isLoadingTeams && managedTeams.length === 0 && (
                        <p className="text-sm text-amber-600 mt-2">
                          No teams found. {isNgbAdmin 
                            ? "Your NGB doesn't have any teams registered yet."
                            : "Please contact your NGB admin to be added as a team manager."}
                        </p>
                      )}
                    </div>
                  </div>

                  {/* Players to Register */}
                  {teamData.selectedTeamId && (
                    <div>
                      <div className="flex items-center justify-between mb-3">
                        <h5 className="text-sm font-semibold text-gray-900">
                          Players to Register
                        </h5>
                        {teamMembers.length > 0 && (
                          <div className="flex items-center">
                            <span className="bg-blue-500 text-white text-xs font-medium px-2 py-1 rounded-full mr-4">
                              {teamData.selectedPlayers.length} selected players
                            </span>
                            <div className="flex">
                              <button
                                type="button"
                                onClick={selectAllPlayers}
                                className="text-xs text-blue-600 hover:underline mr-2"
                              >
                                Select All
                              </button>
                              <button
                                type="button"
                                onClick={deselectAllPlayers}
                                className="text-xs text-gray-600 hover:underline"
                              >
                                Deselect All
                              </button>
                            </div>
                          </div>
                        )}
                      </div>

                      {isLoadingMembers ? (
                        <div className="text-center py-4 text-gray-500">
                          Loading team members...
                        </div>
                      ) : teamMembers.length === 0 ? (
                        <div className="text-center py-4 text-amber-600 text-sm">
                          <p className="font-medium">No team members found.</p>
                          <p className="mt-1 text-gray-600">
                            Referees become team members when they select this team as their 
                            &quot;Playing Team&quot; or &quot;Coaching Team&quot; on their profile.
                          </p>
                        </div>
                      ) : (
                        <div className="border border-gray-200 rounded overflow-y-auto" style={{ maxHeight: '20rem' }}>
                          {teamMembers.map((member) => {
                            const selected = isPlayerSelected(member.userId);
                            return (
                              <div
                                key={member.userId}
                                className={`px-4 py-3 border-b border-gray-100 ${
                                  selected ? "bg-blue-100" : "hover:bg-gray-50"
                                }`}
                              >
                                <label className="flex items-center cursor-pointer">
                                  <input
                                    type="checkbox"
                                    checked={selected}
                                    onChange={() => togglePlayerSelection(member.userId)}
                                    className="w-4 h-4 text-blue-600 border-gray-300 rounded"
                                  />
                                  <span className="ml-3 text-sm text-gray-900 font-medium">
                                    {member.name}
                                  </span>
                                </label>
                                {selected && (
                                  <div className="mt-2 ml-6">
                                    <input
                                      type="text"
                                      value={getPlayerNotes(member.userId)}
                                      onChange={(e) => updatePlayerNotes(member.userId, e.target.value)}
                                      placeholder="Notes (e.g., jersey size, position, medical info)"
                                      className="w-full px-2 py-1 text-xs border border-gray-200 rounded focus:outline-none focus:border-blue-500"
                                    />
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  )}

                  {/* Additional Notes */}
                  <div>
                    <label
                      htmlFor="teamNotes"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Team Notes (Optional)
                    </label>
                    <textarea
                      id="teamNotes"
                      value={teamData.additionalNotes}
                      onChange={(e) =>
                        setTeamData((prev) => ({
                          ...prev,
                          additionalNotes: e.target.value,
                        }))
                      }
                      rows={3}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                      placeholder="General notes for the team registration (e.g., travel arrangements, dietary requirements)..."
                    />
                  </div>

                  {/* Validation Errors */}
                  {teamValidationErrors.length > 0 && teamData.selectedTeamId && (
                    <div className="bg-red-50 border border-red-200 rounded-md p-3">
                      <ul className="text-xs text-red-800 list-disc list-inside">
                        {teamValidationErrors.map((error, i) => (
                          <li key={i}>{error}</li>
                        ))}
                      </ul>
                    </div>
                  )}

                  {/* Info Note */}
                  <p className="text-xs text-gray-500">
                    Fees &amp; roster limits may vary based on selected players.
                  </p>
                </>
              )}
             {mode === "player" && (
                <>
                  {/* Team Selection for Player */}
                  <div>
                    <h5 className="text-sm font-semibold text-gray-900 mb-3">
                      Team Affiliation
                    </h5>
                    <div className="mb-4">
                      <label
                        htmlFor="playerTeamSelect"
                        className="block text-sm font-medium text-gray-700 mb-1"
                      >
                        Select a team (optional)
                      </label>
                      <select
                        id="playerTeamSelect"
                        value={playerData.selectedTeamId}
                        onChange={(e) =>
                          setPlayerData((prev) => ({
                            ...prev,
                            selectedTeamId: e.target.value,
                          }))
                        }
                        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500 bg-white"
                      >
                        <option value="">No team / Free agent</option>
                        {playerTeams.map((team) => (
                          <option key={team.teamId} value={team.teamId}>
                            {team.teamName}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>

                  {/* Contact Information */}
                  <div>
                    <h5 className="text-sm font-semibold text-gray-900 mb-3">
                      Contact Information
                    </h5>

                    <div className="mb-4">
                      <label
                        htmlFor="contactName"
                        className="block text-sm font-medium text-gray-700 mb-1"
                      >
                        Full Name *
                      </label>
                      <input
                        type="text"
                        id="contactName"
                        value={playerData.contactName}
                        onChange={(e) =>
                          setPlayerData((prev) => ({
                            ...prev,
                            contactName: e.target.value,
                          }))
                        }
                        required
                        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                        placeholder="Your full name"
                      />
                    </div>

                    <div className="mb-4">
                      <div className="flex flex-wrap -mx-2">
                        <div className="w-full md:w-1/2 px-2 mb-4 md:mb-0">
                          <label
                            htmlFor="contactEmail"
                            className="block text-sm font-medium text-gray-700 mb-1"
                          >
                            Email *
                          </label>
                          <input
                            type="email"
                            id="contactEmail"
                            value={playerData.contactEmail}
                            onChange={(e) =>
                              setPlayerData((prev) => ({
                                ...prev,
                                contactEmail: e.target.value,
                              }))
                            }
                            required
                            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                            placeholder="email@example.com"
                          />
                        </div>

                        <div className="w-full md:w-1/2 px-2">
                          <label
                            htmlFor="contactPhone"
                            className="block text-sm font-medium text-gray-700 mb-1"
                          >
                            Phone Number *
                          </label>
                          <input
                            type="tel"
                            id="contactPhone"
                            value={playerData.contactPhone}
                            onChange={(e) =>
                              setPlayerData((prev) => ({
                                ...prev,
                                contactPhone: e.target.value,
                              }))
                            }
                            required
                            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                            placeholder="+1 234 567 8900"
                          />
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Additional Notes */}
                  <div>
                    <label
                      htmlFor="playerNotes"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Additional Notes (Optional)
                    </label>
                    <textarea
                      id="playerNotes"
                      value={playerData.additionalNotes}
                      onChange={(e) =>
                        setPlayerData((prev) => ({
                          ...prev,
                          additionalNotes: e.target.value,
                        }))
                      }
                      rows={3}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                      placeholder="Preferred position, availability, etc..."
                    />
                  </div>
                </>
              )}

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
                  disabled={
                    isSubmitting ||
                    (mode === "team" && !isTeamFormValid) ||
                    (mode === "player" && !isPlayerFormValid)
                  }
                  className="px-6 py-2 text-sm font-medium rounded"
                  style={{
                    backgroundColor: (isSubmitting || (mode === "team" && !isTeamFormValid) || (mode === "player" && !isPlayerFormValid)) ? "#90cdf4" : "#3182ce",
                    color: "#ffffff",
                    border: "1px solid #3182ce",
                    cursor: (isSubmitting || (mode === "team" && !isTeamFormValid) || (mode === "player" && !isPlayerFormValid)) ? "not-allowed" : "pointer"
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
