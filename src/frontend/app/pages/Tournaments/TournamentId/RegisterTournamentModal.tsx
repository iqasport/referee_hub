import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useMemo } from "react";
import React from "react";
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
import { isTeamEligible, eligibilityLabel, getApiErrorMessage } from "../../../utils/tournamentUtils";

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
  status?: string;
}

interface TeamRegistrationData {
  selectedTeamId: string;
}

export interface RegisterTournamentModalRef {
  open: (tournament: Tournament) => void;
}

// ── Sub-components ───────────────────────────────────────────────────────────

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

interface TeamRegistrationFormSectionProps {
  tournamentType: string;
  isLoadingTeams: boolean;
  availableTeams: ManagedTeam[];
  registeredManagedTeams: { teamId: string; teamName: string; status: string }[];
  selectedTeamId: string;
  onSelect: (teamId: string) => void;
}
const TeamRegistrationFormSection: React.FC<TeamRegistrationFormSectionProps> = ({
  tournamentType, isLoadingTeams, availableTeams, registeredManagedTeams, selectedTeamId, onSelect,
}) => (
  <>
    {tournamentType !== "Fantasy" && (
      <p className="text-xs text-amber-700 font-medium">
        Eligibility: {eligibilityLabel(tournamentType as Parameters<typeof eligibilityLabel>[0])}
      </p>
    )}
    <div>
      <h5 className="text-sm font-semibold text-gray-900 mb-3">Team Information</h5>
      <div className="mb-4">
        <label htmlFor="teamSelect" className="block text-sm font-medium text-gray-700 mb-1">
          Select an existing team you manage
        </label>
        {isLoadingTeams ? (
          <div className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-gray-500">
            Loading teams...
          </div>
        ) : (
          <select
            id="teamSelect"
            value={selectedTeamId}
            onChange={(e) => onSelect(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500 bg-white"
          >
            <option value="">Select an existing team you manage</option>
            {availableTeams.map((team) => (
              <option key={team.teamId} value={team.teamId}>{team.teamName}</option>
            ))}
          </select>
        )}
        {!isLoadingTeams && availableTeams.length === 0 && (
          <p className="text-sm text-amber-600 mt-2">No eligible teams found for this tournament type.</p>
        )}
        <RegisteredTeamsList registeredTeams={registeredManagedTeams} />
      </div>
    </div>
    <p className="text-xs text-gray-500">
      Once the tournament organizer approves your invite, you&apos;ll be able to manage
      your team&apos;s roster and submit your player list.
    </p>
  </>
);

// ── Custom hook: derived invite / availability state ─────────────────────────

interface RegisterInviteState {
  availableTeams: ManagedTeam[];
  registeredManagedTeams: { teamId: string; teamName: string; status: string }[];
  isTeamFormValid: (selectedTeamId: string) => boolean;
}

function useRegisterInviteState(
  tournament: Tournament | null,
  managedTeams: ManagedTeam[],
  existingInvites: TournamentInviteViewModel[] | undefined,
): RegisterInviteState {
  const teamsWithExistingInvites = useMemo(() => {
    const ids = new Set<string>();
    existingInvites?.forEach((inv) => { if (inv.participantId) ids.add(inv.participantId); });
    return ids;
  }, [existingInvites]);

  const availableTeams = useMemo(() => managedTeams.filter((team) => {
    if (teamsWithExistingInvites.has(team.teamId)) return false;
    if (team.status === "inactive") return false;
    return isTeamEligible(
      team.groupAffiliation as Parameters<typeof isTeamEligible>[0],
      tournament?.type as TournamentType | undefined
    );
  }), [managedTeams, teamsWithExistingInvites, tournament?.type]);

  const registeredManagedTeams = useMemo(
    () => managedTeams
      .filter((t) => teamsWithExistingInvites.has(t.teamId))
      .map((t) => {
        const invite = existingInvites?.find((i) => i.participantId === t.teamId);
        return { teamId: t.teamId, teamName: t.teamName, status: invite?.status || "unknown" };
      }),
    [managedTeams, teamsWithExistingInvites, existingInvites]
  );

  return {
    availableTeams,
    registeredManagedTeams,
    isTeamFormValid: (selectedTeamId: string) =>
      !!selectedTeamId && availableTeams.some((t) => t.teamId === selectedTeamId),
  };
}

// ── Custom hook: collects teams the current user can register ─────────────────
function useManagedTeamsForRegistration() {
  const { data: currentUser } = useGetCurrentUserQuery();

  const userNgbs: string[] = useMemo(() => {
    if (!currentUser?.roles) return [];
    const ngbAdminRoles = (currentUser.roles as UserRole[]).filter((r) => r.roleType === "NgbAdmin");
    const ngbs: string[] = [];
    ngbAdminRoles.forEach((r) => {
      if (typeof r.ngb === "string" && r.ngb) ngbs.push(r.ngb);
      else if (Array.isArray(r.ngb)) ngbs.push(...r.ngb);
    });
    return [...new Set(ngbs)];
  }, [currentUser]);

  const isNgbAdmin = userNgbs.length > 0;
  const primaryNgb = userNgbs[0] ?? "";

  const { data: managedTeamsData, isLoading: isLoadingManagedTeams } = useGetManagedTeamsQuery();
  const { data: ngbTeamsData, isLoading: isLoadingNgbTeams } = useGetNgbTeamsQuery(
    { ngb: primaryNgb, skipPaging: true },
    { skip: !isNgbAdmin || !primaryNgb }
  );

  const managedTeams: ManagedTeam[] = useMemo(() => {
    const teams: ManagedTeam[] = [];
    const addedIds = new Set<string>();
    managedTeamsData?.forEach((team: ManagedTeamViewModel) => {
      if (team.teamId && !addedIds.has(team.teamId)) {
        addedIds.add(team.teamId);
        teams.push({ teamId: team.teamId, teamName: team.teamName ?? `Team ${team.teamId}`, ngb: team.ngb ?? "", groupAffiliation: team.groupAffiliation, status: team.status });
      }
    });
    if (isNgbAdmin) {
      ngbTeamsData?.items?.forEach((team: NgbTeamViewModel) => {
        if (team.teamId && !addedIds.has(team.teamId)) {
          addedIds.add(team.teamId);
          teams.push({ teamId: team.teamId, teamName: team.name ?? `Team ${team.teamId}`, ngb: primaryNgb, groupAffiliation: team.groupAffiliation, status: team.status });
        }
      });
    }
    return teams;
  }, [managedTeamsData, ngbTeamsData, primaryNgb, isNgbAdmin]);

  return {
    managedTeams,
    isLoadingTeams: isLoadingManagedTeams || (isNgbAdmin && isLoadingNgbTeams),
  };
}

// ── Pure helpers ─────────────────────────────────────────────────────────────

function formatTournamentDateRange(startDate: Date | null, endDate: Date | null): string {
  if (!startDate || !endDate) return "";
  return `${startDate.toLocaleDateString("en-US", { month: "short", day: "numeric" })} - ${endDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}`;
}

// ── Action buttons row ────────────────────────────────────────────────────────
interface RegistrationActionButtonsProps {
  isSubmitting: boolean;
  isFormValid: boolean;
  onClose: () => void;
}
const RegistrationActionButtons: React.FC<RegistrationActionButtonsProps> = ({ isSubmitting, isFormValid, onClose }) => {
  const disabled = isSubmitting || !isFormValid;
  return (
    <div className="flex justify-end mt-6 pt-4 border-t border-gray-200">
      <button
        type="button"
        onClick={onClose}
        className="px-6 py-2 text-sm font-medium rounded mr-3"
        style={{ backgroundColor: "#edf2f7", color: "#4a5568", border: "1px solid #e2e8f0" }}
      >
        Cancel
      </button>
      <button
        type="submit"
        disabled={disabled}
        className="px-6 py-2 text-sm font-medium rounded"
        style={{
          backgroundColor: disabled ? "#90cdf4" : "#3182ce",
          color: "#ffffff",
          border: "1px solid #3182ce",
          cursor: disabled ? "not-allowed" : "pointer",
        }}
      >
        {isSubmitting ? "Submitting..." : "Submit Registration"}
      </button>
    </div>
  );
};

const RegisterTournamentModal = forwardRef<RegisterTournamentModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [tournament, setTournament] = useState<Tournament | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [teamData, setTeamData] = useState<TeamRegistrationData>({ selectedTeamId: "" });

  const { managedTeams, isLoadingTeams } = useManagedTeamsForRegistration();
  const { data: existingInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournament?.id || "" },
    { skip: !tournament?.id || !isOpen }
  );

  const {
    availableTeams,
    registeredManagedTeams,
    isTeamFormValid,
  } = useRegisterInviteState(tournament, managedTeams, existingInvites);

  const [createInvite] = useCreateInviteMutation();

  useImperativeHandle(ref, () => ({
    open: (tournamentData: Tournament) => {
      setTournament(tournamentData);
      setTeamData({ selectedTeamId: "" });
      setIsOpen(true);
    },
  }));

  function close() { setIsOpen(false); }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await createInvite({
        tournamentId: tournament?.id || "",
        createInviteModel: { participantType: "team", participantId: teamData.selectedTeamId },
      }).unwrap();
      showAlert(
        `Successfully sent invite for ${availableTeams.find((t) => t.teamId === teamData.selectedTeamId)?.teamName} to ${tournament?.name}! The tournament organizer will review your request.`,
        "success"
      );
      close();
    } catch (error: unknown) {
      console.error("Failed to register for tournament:", error);
      showAlert(getApiErrorMessage(error, "Failed to register for tournament. Please try again."), "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  const formattedDateRange = formatTournamentDateRange(
    tournament ? new Date(tournament.startDate) : null,
    tournament ? new Date(tournament.endDate) : null
  );

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
            <TeamRegistrationFormSection
              tournamentType={tournament?.type || ""}
              isLoadingTeams={isLoadingTeams}
              availableTeams={availableTeams}
              registeredManagedTeams={registeredManagedTeams}
              selectedTeamId={teamData.selectedTeamId}
              onSelect={(teamId) => setTeamData((prev) => ({ ...prev, selectedTeamId: teamId }))}
            />

            <RegistrationActionButtons
              isSubmitting={isSubmitting}
              isFormValid={isTeamFormValid(teamData.selectedTeamId)}
              onClose={close}
            />
          </form>
        </DialogPanel>
      </div>
      </Dialog>
    </>
  );
});

RegisterTournamentModal.displayName = "RegisterTournamentModal";

export default RegisterTournamentModal;
