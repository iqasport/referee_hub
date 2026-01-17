import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useMemo } from "react";
import React from "react";
import {
  useGetCurrentUserQuery,
  useGetNgbTeamsQuery,
  useCreateInviteMutation,
  NgbTeamViewModel,
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
              groupAffiliation: team.groupAffiliation,
            });
          }
        });
      }
      
      return teams;
    }, [teamManagerTeams, isNgbAdmin, ngbTeamsData, primaryNgb]);

    // Team registration form data
    const initialTeamData: TeamRegistrationData = {
      selectedTeamId: "",
    };
    const [teamData, setTeamData] = useState<TeamRegistrationData>(initialTeamData);

    // Create invite mutation
    const [createInvite] = useCreateInviteMutation();

    // Loading state for teams
    const isLoadingTeams = isNgbAdmin && isLoadingNgbTeams;

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



    // Validation
    const isTeamFormValid = !!teamData.selectedTeamId;

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

        const selectedTeam = managedTeams.find(t => t.teamId === teamData.selectedTeamId);
        console.log("Team invite sent:", {
          tournamentId: tournament?.id,
          teamId: teamData.selectedTeamId,
          teamName: selectedTeam?.teamName,
        });

        alert(`Successfully sent invite for ${selectedTeam?.teamName} to ${tournament?.name}! The tournament organizer will review your request.`);

        close();
      } catch (error: any) {
        console.error("Failed to register for tournament:", error);
        const errorMessage = error?.data?.error || error?.message || "Failed to register for tournament. Please try again.";
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
