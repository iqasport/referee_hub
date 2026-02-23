import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useMemo, useEffect } from "react";
import React from "react";
import classnames from "classnames";
import StatusBadge from "../../../components/StatusBadge";
import {
  useGetCurrentUserQuery,
  useGetManagedTeamsQuery,
  useGetNgbTeamsQuery,
  useCreateInviteMutation,
  useGetTournamentInvitesQuery,
  ManagedTeamViewModel,
  NgbTeamViewModel,
  TournamentType,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import { isTeamEligible, eligibilityLabel } from "../../../utils/tournamentUtils";

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
  allowsIndividualRegistration?: boolean;
  allowsTeamRegistration?: boolean;
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

// ── Sub-components ───────────────────────────────────────────────────────────

interface RegistrationStatusNoticeProps {
  invite: TournamentInviteViewModel;
}
const RegistrationStatusNotice: React.FC<RegistrationStatusNoticeProps> = ({ invite }) => {
  const cls =
    invite.status === "approved"
      ? "bg-green-50 border border-green-200 text-green-800"
      : invite.status === "rejected"
      ? "bg-red-50 border border-red-200 text-red-800"
      : "bg-yellow-50 border border-yellow-200 text-yellow-800";
  return (
    <div className={`rounded p-3 text-sm font-medium ${cls}`}>
      {invite.status === "approved" && "✓ You have been accepted to this tournament as an individual player."}
      {invite.status === "rejected" && "✗ Your individual registration was rejected by the tournament organizer."}
      {(invite.status === "pending" || !invite.status) && "⏳ Your individual registration is pending the tournament organizer's decision."}
    </div>
  );
};

interface RegistrationModeToggleProps {
  mode: "team" | "individual";
  setMode: (m: "team" | "individual") => void;
}
const RegistrationModeToggle: React.FC<RegistrationModeToggleProps> = ({ mode, setMode }) => (
  <div className="mb-2">
    <button type="button" onClick={() => setMode("team")} className={classnames("button-tab", { ["active-button-tab"]: mode === "team" })}>
      Register a Team
    </button>
    <button type="button" onClick={() => setMode("individual")} className={classnames("button-tab", { ["active-button-tab"]: mode === "individual" })}>
      Register as Individual
    </button>
  </div>
);

const IndividualRegistrationPanel: React.FC = () => (
  <div>
    <h5 className="text-sm font-semibold text-gray-900 mb-3">Individual Registration</h5>
    <p className="text-sm text-gray-600">
      You will be registered as an individual player. The tournament organizer will review your request.
    </p>
  </div>
);

interface RegisteredTeamsListProps {
  registeredTeams: { teamId: string; teamName: string; status: string }[];
}
const RegisteredTeamsList: React.FC<RegisteredTeamsListProps> = ({ registeredTeams }) => {
  if (registeredTeams.length === 0) return null;
  return (
    <div className="mt-3 p-3 rounded" style={{ backgroundColor: "#f3f4f6" }}>
      <p className="text-xs text-gray-600 mb-2">Teams already registered:</p>
      <div className="text-sm text-gray-700">
        {registeredTeams.map((t) => (
          <div key={t.teamId} className="flex items-center justify-between py-1">
            <span>{t.teamName}</span>
            <StatusBadge status={t.status} />
          </div>
        ))}
      </div>
    </div>
  );
};

// ── Main component ────────────────────────────────────────────────────────────

const RegisterTournamentModal = forwardRef<RegisterTournamentModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [tournament, setTournament] = useState<Tournament | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [registrationMode, setRegistrationMode] = useState<"team" | "individual">("team");

  const { data: currentUser } = useGetCurrentUserQuery();

  // Check if user is NGB Admin and get their NGB(s)
  const userNgbs: string[] = useMemo(() => {
    if (!currentUser?.roles) return [];
    const ngbAdminRoles = (currentUser.roles as UserRole[]).filter(
      (r) => r.roleType === "NgbAdmin"
    );
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

  // Filter managed teams to only show those without existing invites,
  // eligible for this tournament type, and respecting allowsTeamRegistration
  const availableTeams = useMemo(() => {
    return managedTeams.filter((team) => {
      if (teamsWithExistingInvites.has(team.teamId)) return false;
      // Respect allowsTeamRegistration flag for Fantasy tournaments
      if (tournament?.type === "Fantasy" && !tournament.allowsTeamRegistration) return false;
      // Filter by tournament type eligibility
      return isTeamEligible(
        team.groupAffiliation as Parameters<typeof isTeamEligible>[0],
        tournament?.type as TournamentType | undefined
      );
    });
  }, [managedTeams, teamsWithExistingInvites, tournament?.type, tournament?.allowsTeamRegistration]);

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
      // Default mode: team if allowed, else individual
      const defaultMode = (tournamentData.allowsTeamRegistration !== false) ? "team" : "individual";
      setRegistrationMode(defaultMode);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
  }

  // Validation - ensure selected team is in availableTeams (not already registered)
  const isTeamFormValid =
    !!teamData.selectedTeamId && availableTeams.some((t) => t.teamId === teamData.selectedTeamId);

  // For individual mode: check user is not already registered
  const existingIndividualInvite = existingInvites?.find(
    (i) => i.participantType === "player" && i.participantId === currentUser?.userId
  );
  const hasExistingIndividualInvite = !!existingIndividualInvite;

  const isIndividualFormValid = !hasExistingIndividualInvite;

  // Pre-computed derived state to reduce JSX complexity
  const shouldShowModeToggle =
    tournament?.type === "Fantasy" &&
    tournament.allowsIndividualRegistration === true &&
    tournament.allowsTeamRegistration !== false &&
    !hasExistingIndividualInvite;

  const isAlreadyRegisteredIndividualOnly =
    tournament?.type === "Fantasy" &&
    tournament.allowsIndividualRegistration === true &&
    tournament.allowsTeamRegistration === false &&
    hasExistingIndividualInvite;

  const registeredManagedTeams = useMemo(
    () =>
      managedTeams
        .filter((t) => teamsWithExistingInvites.has(t.teamId))
        .map((t) => {
          const invite = existingInvites?.find((i) => i.participantId === t.teamId);
          return { teamId: t.teamId, teamName: t.teamName, status: invite?.status || "unknown" };
        }),
    [managedTeams, teamsWithExistingInvites, existingInvites]
  );

  // If existing invites load and reveal the player already registered individually,
  // and the modal is currently in individual mode, switch back to team mode.
  useEffect(() => {
    if (hasExistingIndividualInvite && registrationMode === "individual") {
      setRegistrationMode("team");
    }
  }, [hasExistingIndividualInvite, registrationMode]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (registrationMode === "individual") {
        // Individual player registration (Fantasy tournaments only)
        await createInvite({
          tournamentId: tournament?.id || "",
          createInviteModel: {
            participantType: "player",
            participantId: currentUser?.userId || "",
          },
        }).unwrap();

        showAlert(
          `Successfully registered as an individual player for ${tournament?.name}! The tournament organizer will review your request.`,
          "success"
        );
      } else {
        // Team registration
        await createInvite({
          tournamentId: tournament?.id || "",
          createInviteModel: {
            participantType: "team",
            participantId: teamData.selectedTeamId,
          },
        }).unwrap();

        const selectedTeam = availableTeams.find((t) => t.teamId === teamData.selectedTeamId);
        showAlert(
          `Successfully sent invite for ${selectedTeam?.teamName} to ${tournament?.name}! The tournament organizer will review your request.`,
          "success"
        );
      }

      close();
    } catch (error: unknown) {
      console.error("Failed to register for tournament:", error);
      const errorMessage =
        error instanceof Error
          ? error.message
          : "Failed to register for tournament. Please try again.";
      showAlert(errorMessage, "error");
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
        })} - ${endDate.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
          year: "numeric",
        })}`
      : "";

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert
          message={alertState.message}
          type={alertState.type}
          onClose={hideAlert}
        />
      )}
      <Dialog open={isOpen && !!tournament} as="div" className="relative z-50" onClose={close}>
      <div
        className="fixed inset-0"
        style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
        aria-hidden="true"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel
          className="relative w-full max-w-2xl rounded bg-white p-6 shadow-xl my-8 overflow-y-auto"
          style={{ maxHeight: "90vh" }}
        >
          <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-1">
            Register for Tournament
          </DialogTitle>

          {/* Tournament Info */}
          <div className="text-sm text-gray-600 mb-4 flex flex-wrap items-center gap-2">
            <span className="font-medium text-gray-800">{tournament?.name}</span>
            <span className="text-gray-400">•</span>
            <span>{formattedDateRange}</span>
            {tournament?.city && (
              <>
                <span className="text-gray-400">•</span>
                <span>{tournament.city}</span>
              </>
            )}
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">

            {/* Individual registration status notice */}
            {existingIndividualInvite && (
              <RegistrationStatusNotice invite={existingIndividualInvite} />
            )}

            {/* Mode toggle (Fantasy + both types + not already registered) */}
            {shouldShowModeToggle && (
              <RegistrationModeToggle mode={registrationMode} setMode={setRegistrationMode} />
            )}

            {/* Form fields — hidden when individual-only and player is already registered */}
            {!isAlreadyRegisteredIndividualOnly && (
            <>

            {/* Individual registration panel */}
            {registrationMode === "individual" ? (
              <IndividualRegistrationPanel />
            ) : (
              <>
                {/* Team Selection */}
                {tournament?.type !== "Fantasy" && (
                  <p className="text-xs text-amber-700 font-medium">
                    Eligibility: {eligibilityLabel(tournament?.type as Parameters<typeof eligibilityLabel>[0])}
                  </p>
                )}
                <div>
                  <h5 className="text-sm font-semibold text-gray-900 mb-3">Team Information</h5>
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
                    {!isLoadingTeams && availableTeams.length === 0 && (
                      <p className="text-sm text-amber-600 mt-2">
                        No eligible teams found for this tournament type.
                      </p>
                    )}
                    <RegisteredTeamsList registeredTeams={registeredManagedTeams} />
                  </div>
                </div>

                {/* Info Note */}
                <p className="text-xs text-gray-500">
                  Once the tournament organizer approves your invite, you&apos;ll be able to manage
                  your team&apos;s roster and submit your player list.
                </p>
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
                  border: "1px solid #e2e8f0",
                }}
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting || (registrationMode === "team" ? !isTeamFormValid : !isIndividualFormValid)}
                className="px-6 py-2 text-sm font-medium rounded"
                style={{
                  backgroundColor: (isSubmitting || (registrationMode === "team" ? !isTeamFormValid : !isIndividualFormValid)) ? "#90cdf4" : "#3182ce",
                  color: "#ffffff",
                  border: "1px solid #3182ce",
                  cursor: (isSubmitting || (registrationMode === "team" ? !isTeamFormValid : !isIndividualFormValid)) ? "not-allowed" : "pointer",
                }}
              >
                {isSubmitting ? "Submitting..." : "Submit Registration"}
              </button>
            </div>
            </>
            )}

            {/* Close button — always shown when individual-only and already registered */}
            {isAlreadyRegisteredIndividualOnly && (
            <div className="flex justify-end mt-6 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={close}
                className="px-6 py-2 text-sm font-medium rounded mr-3"
                style={{
                  backgroundColor: "#edf2f7",
                  color: "#4a5568",
                  border: "1px solid #e2e8f0",
                }}
              >
                Close
              </button>
            </div>
            )}
          </form>
        </DialogPanel>
      </div>
      </Dialog>
    </>
  );
});

RegisterTournamentModal.displayName = "RegisterTournamentModal";

export default RegisterTournamentModal;
