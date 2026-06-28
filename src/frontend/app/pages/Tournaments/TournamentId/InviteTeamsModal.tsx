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
  TournamentType,
  TournamentViewModel,
} from "../../../store/serviceApi";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import {
  isTeamEligible,
  eligibilityLabel,
  getApiErrorMessage,
  getEligibleAffiliations,
  getUnavailableTeamIds,
  filterAndSearchTeams,
  getTeamStatusStyle,
} from "../../../utils/tournamentUtils";

export interface InviteTeamsModalRef {
  open: (tournament: TournamentViewModel) => void;
}

// Sub-components for modal sections
interface NgbSelectorProps {
  selectedNgb: string;
  isLoadingNgbs: boolean;
  filteredNgbs?: { countryCode?: string; name?: string }[];
  onNgbChange: (ngb: string) => void;
}

const NgbSelector: React.FC<NgbSelectorProps> = ({
  selectedNgb,
  isLoadingNgbs,
  filteredNgbs,
  onNgbChange,
}) => (
  <div className="mb-4">
    <label className="block text-sm font-medium text-gray-700 mb-1">
      Select Country/Region (NGB)
    </label>
    {isLoadingNgbs ? (
      <div className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-500">
        Loading regions...
      </div>
    ) : (
      <select
        value={selectedNgb}
        onChange={(e) => onNgbChange(e.target.value)}
        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
      >
        <option value="">Select a region to browse teams...</option>
        {filteredNgbs?.map((ngb) => (
          <option key={ngb.countryCode} value={ngb.countryCode}>
            {ngb.name} ({ngb.countryCode})
          </option>
        ))}
      </select>
    )}
  </div>
);

interface TeamSearchProps {
  searchFilter: string;
  selectedNgb: string;
  onSearchChange: (filter: string) => void;
}

const TeamSearch: React.FC<TeamSearchProps> = ({
  searchFilter,
  selectedNgb,
  onSearchChange,
}) => (
  <div className="mb-4">
    <input
      type="text"
      placeholder="Search teams..."
      value={searchFilter}
      onChange={(e) => onSearchChange(e.target.value)}
      disabled={!selectedNgb}
      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
    />
  </div>
);

interface AvailableTeamsProps {
  selectedNgb: string;
  isLoadingTeams: boolean;
  availableTeams: NgbTeamViewModel[];
  selectedTeamId: string;
  searchFilter: string;
  tournamentType?: TournamentType;
  onTeamSelect: (teamId: string) => void;
}

const AvailableTeamsSection: React.FC<AvailableTeamsProps> = ({
  selectedNgb,
  isLoadingTeams,
  availableTeams,
  selectedTeamId,
  searchFilter,
  tournamentType,
  onTeamSelect,
}) => (
  <div className="mb-6">
    <h4 className="text-sm font-semibold text-gray-700 mb-3">Available Teams</h4>
    {!selectedNgb ? (
      <div className="text-center py-8 text-gray-500">
        Please select a region above to browse available teams
      </div>
    ) : isLoadingTeams ? (
      <div className="text-center py-8 text-gray-500">Loading teams...</div>
    ) : availableTeams.length === 0 ? (
      <div className="text-center py-8 text-gray-500">
        {searchFilter
          ? "No teams match your search"
          : `No eligible teams found in this region.${
              tournamentType
                ? ` This is a ${tournamentType} tournament - only teams with matching affiliation (${eligibilityLabel(
                    tournamentType
                  )}) will appear here.`
                : " No available teams to invite."
            }`}
      </div>
    ) : (
      <div className="space-y-2 max-h-48 overflow-y-auto">
        {availableTeams.map((team) => (
          <div
            key={team.teamId}
            onClick={() => onTeamSelect(team.teamId || "")}
            className={`p-3 border rounded-lg cursor-pointer transition-colors ${
              selectedTeamId === team.teamId
                ? "border-blue-500 bg-blue-50"
                : "border-gray-200 hover:border-gray-300 hover:bg-gray-50"
            }`}
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                {team.logoUri && (
                  <img
                    src={team.logoUri}
                    alt={`${team.name} logo`}
                    className="w-8 h-8 rounded-full object-cover flex-shrink-0 border border-gray-200"
                  />
                )}
                <div>
                  <p className="font-medium text-gray-900">{team.name}</p>
                  <p className="text-xs text-gray-500">
                    {team.groupAffiliation || "Team"}
                  </p>
                </div>
              </div>
              {selectedTeamId === team.teamId && (
                <svg className="w-5 h-5 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fillRule="evenodd"
                    d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                    clipRule="evenodd"
                  />
                </svg>
              )}
            </div>
          </div>
        ))}
      </div>
    )}
  </div>
);

interface SentInvitesProps {
  existingInvites?: { participantId?: string; participantName?: string; status?: string }[];
}

const SentInvitesList: React.FC<SentInvitesProps> = ({ existingInvites }) => {
  if (!existingInvites || existingInvites.length === 0) return null;

  return (
    <div>
      <h4 className="text-sm font-semibold text-gray-700 mb-3">Sent Invites</h4>
      <div className="space-y-2 max-h-32 overflow-y-auto">
        {existingInvites.map((invite) => {
          const statusStyle = getTeamStatusStyle(invite.status);
          return (
            <div
              key={invite.participantId}
              className="p-3 border border-gray-200 rounded-lg bg-gray-50"
            >
              <div className="flex items-center justify-between">
                <p className="font-medium text-gray-900">{invite.participantName}</p>
                <span
                  className="px-2 py-1 rounded-full text-xs font-semibold"
                  style={{ backgroundColor: statusStyle.bg, color: statusStyle.color }}
                >
                  {invite.status
                    ? invite.status.charAt(0).toUpperCase() + invite.status.slice(1)
                    : "Unknown"}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

interface InviteModalFooterProps {
  selectedTeamId: string;
  isSubmitting: boolean;
  onClose: () => void;
  onInvite: () => void;
}

const InviteModalFooter: React.FC<InviteModalFooterProps> = ({
  selectedTeamId,
  isSubmitting,
  onClose,
  onInvite,
}) => (
  <div className="p-6 border-t border-gray-200 flex justify-end gap-3">
    <button
      onClick={onClose}
      className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
    >
      Close
    </button>
    <button
      onClick={onInvite}
      disabled={!selectedTeamId || isSubmitting}
      className="btn btn-primary"
    >
      {isSubmitting ? "Sending..." : "Send Invite"}
    </button>
  </div>
);

const InviteTeamsModal = forwardRef<InviteTeamsModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [tournament, setTournament] = useState<TournamentViewModel | null>(null);
  const [selectedNgb, setSelectedNgb] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchFilter, setSearchFilter] = useState("");
  const [selectedTeamId, setSelectedTeamId] = useState<string>("");

  // Fetch list of NGBs (filtered by tournament eligibility when applicable)
  const eligibleAffiliations = getEligibleAffiliations(tournament?.type);
  const { data: ngbsData, isLoading: isLoadingNgbs } = useGetNgbsQuery({ skipPaging: true }, { skip: !isOpen });
  const { data: eligibleNgbCodes } = useGetEligibleNgbsQuery(
    { groupAffiliations: eligibleAffiliations ?? [] },
    { skip: !isOpen || eligibleAffiliations === null }
  );
  const eligibleNgbCodeSet = useMemo(() => new Set(eligibleNgbCodes ?? []), [eligibleNgbCodes]);
  const filteredNgbs = useMemo(
    () =>
      eligibleAffiliations === null
        ? ngbsData?.items
        : ngbsData?.items?.filter((ngb) => eligibleNgbCodeSet.has(ngb.countryCode ?? "")),
    [eligibleAffiliations, ngbsData?.items, eligibleNgbCodeSet]
  );

  // Fetch teams from selected NGB
  const { data: teamsData, isLoading: isLoadingTeams } = useGetNgbTeamsQuery(
    { ngb: selectedNgb, skipPaging: true },
    { skip: !selectedNgb || !isOpen }
  );

  // Fetch existing invites for this tournament
  const { data: existingInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournament?.id || "" },
    { skip: !tournament?.id || !isOpen }
  );

  // Fetch existing participants
  const { data: participants } = useGetParticipantsQuery(
    { tournamentId: tournament?.id || "" },
    { skip: !tournament?.id || !isOpen }
  );

  const [createInvite] = useCreateInviteMutation();

  // Get team IDs that already have invites or are participants
  const unavailableTeamIds = useMemo(
    () => getUnavailableTeamIds(existingInvites, participants),
    [existingInvites, participants]
  );

  // Filter teams - exclude unavailable/ineligible teams and apply search
  const availableTeams = useMemo(
    () =>
      filterAndSearchTeams(
        teamsData?.items,
        unavailableTeamIds,
        searchFilter,
        tournament?.type
      ),
    [teamsData?.items, unavailableTeamIds, searchFilter, tournament?.type]
  );

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
        createInviteModel: {
          participantType: "team",
          participantId: selectedTeamId,
        },
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

  const getStatusStyle = (status?: string) => {
    return getTeamStatusStyle(status);
  };

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}
      <Dialog open={isOpen} as="div" className="relative z-50" onClose={close}>
        <div
          className="fixed inset-0"
          style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
          aria-hidden="true"
        />
        <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
          <DialogPanel className="relative w-full max-w-2xl rounded-xl bg-white shadow-xl my-8 max-h-screen overflow-hidden flex flex-col">
            {/* Header */}
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <DialogTitle as="h3" className="text-xl font-semibold text-gray-900">
                  Invite Teams
                </DialogTitle>
                <button
                  onClick={close}
                  className="text-gray-400 hover:text-gray-600"
                  style={{ fontSize: "24px", lineHeight: 1 }}
                >
                  ×
                </button>
              </div>
              <p className="text-sm text-gray-600 mt-1">
                Invite teams to participate in {tournament?.name}
              </p>
              {tournament?.type && (
                <p className="text-xs text-amber-700 mt-1 font-medium">
                  Eligibility: {eligibilityLabel(tournament.type)}
                </p>
              )}
            </div>

            {/* Content */}
            <div className="p-6 overflow-y-auto" style={{ maxHeight: "60vh" }}>
              <NgbSelector
                selectedNgb={selectedNgb}
                isLoadingNgbs={isLoadingNgbs}
                filteredNgbs={filteredNgbs}
                onNgbChange={(ngb) => {
                  setSelectedNgb(ngb);
                  setSelectedTeamId("");
                }}
              />

              <TeamSearch
                searchFilter={searchFilter}
                selectedNgb={selectedNgb}
                onSearchChange={setSearchFilter}
              />

              <AvailableTeamsSection
                selectedNgb={selectedNgb}
                isLoadingTeams={isLoadingTeams}
                availableTeams={availableTeams}
                selectedTeamId={selectedTeamId}
                searchFilter={searchFilter}
                tournamentType={tournament?.type}
                onTeamSelect={setSelectedTeamId}
              />

              <SentInvitesList existingInvites={existingInvites} />
            </div>

            {/* Footer */}
            <InviteModalFooter
              selectedTeamId={selectedTeamId}
              isSubmitting={isSubmitting}
              onClose={close}
              onInvite={handleInviteTeam}
            />
          </DialogPanel>
        </div>
      </Dialog>
    </>
  );
});

InviteTeamsModal.displayName = "InviteTeamsModal";

export default InviteTeamsModal;
