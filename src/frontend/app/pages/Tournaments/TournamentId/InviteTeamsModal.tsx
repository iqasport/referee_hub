import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useMemo } from "react";
import React from "react";
import {
  useGetNgbTeamsQuery,
  useGetNgbsQuery,
  useGetEligibleNgbsQuery,
  useCreateInviteMutation,
  useGetTournamentInvitesQuery,
  useGetParticipantsQuery,
  NgbTeamViewModel,
  TournamentViewModel,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import { isTeamEligible, eligibilityLabel, getApiErrorMessage, getEligibleAffiliations } from "../../../utils/tournamentUtils";

export interface InviteTeamsModalRef {
  open: (tournament: TournamentViewModel) => void;
}

// ── Data hook ────────────────────────────────────────────────────────────────

function useInviteTeamsData(
  tournament: TournamentViewModel | null,
  isOpen: boolean,
  selectedNgb: string,
  searchFilter: string,
) {
  const eligibleAffiliations = getEligibleAffiliations(tournament?.type);
  const { data: ngbsData, isLoading: isLoadingNgbs } = useGetNgbsQuery({ skipPaging: true }, { skip: !isOpen });
  const { data: eligibleNgbCodes } = useGetEligibleNgbsQuery(
    { groupAffiliations: eligibleAffiliations ?? [] },
    { skip: !isOpen || eligibleAffiliations === null },
  );
  const filteredNgbs = eligibleAffiliations === null
    ? ngbsData?.items
    : ngbsData?.items?.filter((ngb) => eligibleNgbCodes?.includes(ngb.countryCode ?? ""));

  const { data: teamsData, isLoading: isLoadingTeams } = useGetNgbTeamsQuery(
    { ngb: selectedNgb, skipPaging: true },
    { skip: !selectedNgb || !isOpen },
  );
  const { data: existingInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournament?.id || "" },
    { skip: !tournament?.id || !isOpen },
  );
  const { data: participants } = useGetParticipantsQuery(
    { tournamentId: tournament?.id || "" },
    { skip: !tournament?.id || !isOpen },
  );

  const unavailableTeamIds = useMemo(() => {
    const ids = new Set<string>();
    existingInvites?.forEach((invite) => { if (invite.participantId) ids.add(invite.participantId); });
    participants?.forEach((participant) => { if (participant.teamId) ids.add(participant.teamId); });
    return ids;
  }, [existingInvites, participants]);

  const availableTeams = useMemo(() => {
    if (!teamsData?.items) return [];
    return teamsData.items.filter((team: NgbTeamViewModel) => {
      if (!team.teamId || unavailableTeamIds.has(team.teamId)) return false;
      if (team.status === "inactive") return false;
      if (!isTeamEligible(team.groupAffiliation, tournament?.type)) return false;
      if (searchFilter) return (team.name || "").toLowerCase().includes(searchFilter.toLowerCase());
      return true;
    });
  }, [teamsData, unavailableTeamIds, searchFilter, tournament?.type]);

  return { isLoadingNgbs, filteredNgbs, eligibleAffiliations, isLoadingTeams, existingInvites, availableTeams };
}

// ── Sub-components ────────────────────────────────────────────────────────────

interface TeamPickerSectionProps {
  selectedNgb: string;
  isLoadingTeams: boolean;
  availableTeams: NgbTeamViewModel[];
  selectedTeamId: string;
  tournament: TournamentViewModel | null;
  onSelectTeam: (id: string) => void;
}
const TeamPickerSection: React.FC<TeamPickerSectionProps> = ({
  selectedNgb, isLoadingTeams, availableTeams, selectedTeamId, tournament, onSelectTeam,
}) => (
  <div className="mb-6">
    <h4 className="text-sm font-semibold text-gray-700 mb-3">Available Teams</h4>
    {!selectedNgb ? (
      <div className="text-center py-8 text-gray-500">Please select a region above to browse available teams</div>
    ) : isLoadingTeams ? (
      <div className="text-center py-8 text-gray-500">Loading teams...</div>
    ) : availableTeams.length === 0 ? (
      <div className="text-center py-8 text-gray-500">
        {`No eligible teams found in this region.${tournament?.type ? ` This is a ${tournament.type} tournament — only teams with matching affiliation (${eligibilityLabel(tournament.type)}) will appear here.` : " No available teams to invite."}`}
      </div>
    ) : (
      <div className="space-y-2 max-h-48 overflow-y-auto">
        {availableTeams.map((team) => (
          <div
            key={team.teamId}
            onClick={() => onSelectTeam(team.teamId || "")}
            className={`p-3 border rounded-lg cursor-pointer transition-colors ${selectedTeamId === team.teamId ? "border-blue-500 bg-blue-50" : "border-gray-200 hover:border-gray-300 hover:bg-gray-50"}`}
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium text-gray-900">{team.name}</p>
                <p className="text-xs text-gray-500">{team.groupAffiliation || "Team"}</p>
              </div>
              {selectedTeamId === team.teamId && (
                <svg className="w-5 h-5 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
              )}
            </div>
          </div>
        ))}
      </div>
    )}
  </div>
);

const statusStyle = (status?: string) =>
  status === "pending" ? { bg: "#fef3c7", color: "#92400e" }
  : status === "approved" ? { bg: "#d1fae5", color: "#065f46" }
  : { bg: "#fee2e2", color: "#991b1b" };

interface SentInvitesListProps { invites: TournamentInviteViewModel[] }
const SentInvitesList: React.FC<SentInvitesListProps> = ({ invites }) => (
  <div>
    <h4 className="text-sm font-semibold text-gray-700 mb-3">Sent Invites</h4>
    <div className="space-y-2 max-h-32 overflow-y-auto">
      {invites.map((invite) => {
        const s = statusStyle(invite.status);
        return (
          <div key={invite.participantId} className="p-3 border border-gray-200 rounded-lg bg-gray-50">
            <div className="flex items-center justify-between">
              <p className="font-medium text-gray-900">{invite.participantName}</p>
              <span className="px-2 py-1 rounded-full text-xs font-semibold" style={{ backgroundColor: s.bg, color: s.color }}>
                {invite.status ? invite.status.charAt(0).toUpperCase() + invite.status.slice(1) : "Unknown"}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  </div>
);

// ── Main component ────────────────────────────────────────────────────────────

const InviteTeamsModal = forwardRef<InviteTeamsModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [tournament, setTournament] = useState<TournamentViewModel | null>(null);
  const [selectedNgb, setSelectedNgb] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchFilter, setSearchFilter] = useState("");
  const [selectedTeamId, setSelectedTeamId] = useState<string>("");

  const { isLoadingNgbs, filteredNgbs, eligibleAffiliations, isLoadingTeams, existingInvites, availableTeams } =
    useInviteTeamsData(tournament, isOpen, selectedNgb, searchFilter);

  const [createInvite] = useCreateInviteMutation();

  useImperativeHandle(ref, () => ({
    open: (tournamentData: TournamentViewModel) => {
      setTournament(tournamentData);
      setSelectedNgb("");
      setSearchFilter("");
      setSelectedTeamId("");
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
    setSelectedTeamId("");
    setSearchFilter("");
  }

  async function handleInviteTeam() {
    if (!selectedTeamId || !tournament?.id) return;
    setIsSubmitting(true);
    try {
      await createInvite({
        tournamentId: tournament.id,
        createInviteModel: { participantType: "team", participantId: selectedTeamId },
      }).unwrap();
      const selectedTeam = availableTeams.find((t) => t.teamId === selectedTeamId);
      showAlert(`Successfully sent invite to ${selectedTeam?.name || "team"}!`, "success");
      setSelectedTeamId("");
    } catch (error) {
      console.error("Failed to invite team:", error);
      showAlert(getApiErrorMessage(error, "Failed to send invite. Please try again."), "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}
      <Dialog open={isOpen} as="div" className="relative z-50" onClose={close}>
        <div className="fixed inset-0" style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }} aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
          <DialogPanel className="relative w-full max-w-2xl rounded-xl bg-white shadow-xl my-8 max-h-screen overflow-hidden flex flex-col">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <DialogTitle as="h3" className="text-xl font-semibold text-gray-900">Invite Teams</DialogTitle>
                <button onClick={close} className="text-gray-400 hover:text-gray-600" style={{ fontSize: "24px", lineHeight: 1 }}>×</button>
              </div>
              <p className="text-sm text-gray-600 mt-1">Invite teams to participate in {tournament?.name}</p>
              {tournament?.type && (
                <p className="text-xs text-amber-700 mt-1 font-medium">Eligibility: {eligibilityLabel(tournament.type)}</p>
              )}
            </div>

            <div className="p-6 overflow-y-auto" style={{ maxHeight: "60vh" }}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-1">Select Country/Region (NGB)</label>
                {isLoadingNgbs ? (
                  <div className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-500">Loading regions...</div>
                ) : (
                  <select
                    value={selectedNgb}
                    onChange={(e) => { setSelectedNgb(e.target.value); setSelectedTeamId(""); }}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                  >
                    <option value="">Select a region to browse teams...</option>
                    {filteredNgbs?.map((ngb) => (
                      <option key={ngb.countryCode} value={ngb.countryCode}>{ngb.name} ({ngb.countryCode})</option>
                    ))}
                  </select>
                )}
              </div>

              <div className="mb-4">
                <input
                  type="text"
                  placeholder="Search teams..."
                  value={searchFilter}
                  onChange={(e) => setSearchFilter(e.target.value)}
                  disabled={!selectedNgb}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                />
              </div>

              <TeamPickerSection
                selectedNgb={selectedNgb}
                isLoadingTeams={isLoadingTeams}
                availableTeams={availableTeams}
                selectedTeamId={selectedTeamId}
                tournament={tournament}
                onSelectTeam={setSelectedTeamId}
              />

              {existingInvites && existingInvites.length > 0 && (
                <SentInvitesList invites={existingInvites} />
              )}
            </div>

            <div className="p-6 border-t border-gray-200 flex justify-end gap-3">
              <button onClick={close} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200">Close</button>
              <button onClick={handleInviteTeam} disabled={!selectedTeamId || isSubmitting} className="btn btn-primary">
                {isSubmitting ? "Sending..." : "Send Invite"}
              </button>
            </div>
          </DialogPanel>
        </div>
      </Dialog>
    </>
  );
});

InviteTeamsModal.displayName = "InviteTeamsModal";

export default InviteTeamsModal;
