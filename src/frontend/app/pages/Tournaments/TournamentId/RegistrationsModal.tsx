import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useRef, useMemo } from "react";
import React from "react";
import {
  useGetTournamentInvitesQuery,
  useRespondToInviteMutation,
  useDeleteInviteMutation,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import StatusBadge from "../../../components/StatusBadge";
import ActionButtonPair from "../../../components/ActionButtonPair";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import RosterViewModal, { RosterViewModalRef } from "./RosterViewModal";
import { getApiErrorMessage } from "../../../utils/tournamentUtils";

export interface RegistrationsModalRef {
  open: (tournamentId: string, tournamentName: string) => void;
}

/** Renders a single team invite card inside the detail view. */
interface TeamDetailViewProps {
  invite: TournamentInviteViewModel;
  isSubmitting: boolean;
  onApprove: (id: string, name: string) => void;
  onDeny: (id: string, name: string) => void;
  onDelete: (id: string, name: string) => void;
}
const TeamDetailView: React.FC<TeamDetailViewProps> = ({
  invite,
  isSubmitting,
  onApprove,
  onDeny,
  onDelete,
}) => (
  <div>
    <div className="bg-gray-50 rounded-lg p-4 mb-4">
      <p className="text-sm text-gray-600 mb-1">Team Name</p>
      <p className="text-lg font-semibold text-gray-900">{invite.participantName}</p>
    </div>
    <div className="bg-gray-50 rounded-lg p-4 mb-4">
      <p className="text-sm text-gray-600 mb-1">Request Submitted</p>
      <p className="text-sm font-semibold text-gray-900">
        {new Date(invite.createdAt).toLocaleDateString("en-US", {
          month: "short", day: "numeric", year: "numeric", hour: "2-digit", minute: "2-digit",
        })}
      </p>
    </div>
    <div className="bg-gray-50 rounded-lg p-4 mb-6">
      <p className="text-sm text-gray-600 mb-2">Status</p>
      <div className="flex items-center">
        <StatusBadge status={invite.status || "unknown"} />
        {invite.status !== "pending" && invite.tournamentManagerApproval?.date && (
          <span className="text-xs text-gray-600 ml-2">
            on {new Date(invite.tournamentManagerApproval.date).toLocaleDateString("en-US", { month: "short", day: "numeric" })}
          </span>
        )}
      </div>
    </div>
    {invite.status === "pending" && invite.tournamentManagerApproval?.status === "pending" && (
      <ActionButtonPair
        onAccept={() => onApprove(invite.participantId ?? "", invite.participantName || "")}
        onDecline={() => onDeny(invite.participantId ?? "", invite.participantName || "")}
        isLoading={isSubmitting}
        acceptLabel="Approve"
        declineLabel="Deny"
        loadingLabel="Processing..."
      />
    )}
    {invite.status === "pending" &&
      invite.tournamentManagerApproval?.status === "approved" &&
      invite.participantApproval?.status === "pending" && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <p className="text-sm text-blue-800">⏳ Waiting for the team to accept or decline this invitation.</p>
        </div>
      )}
    {invite.status === "rejected" && (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <p className="text-sm text-red-800 mb-3">
          This registration was rejected. You can remove this invite to allow the team to be re-invited.
        </p>
        <button
          onClick={() => onDelete(invite.participantId ?? "", invite.participantName || "")}
          disabled={isSubmitting}
          className="px-4 py-2 text-sm font-medium rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
        >
          {isSubmitting ? "Removing..." : "Remove Invite"}
        </button>
      </div>
    )}
  </div>
);

interface TeamInviteListRowProps {
  invite: TournamentInviteViewModel;
  getPendingLabel: (invite: TournamentInviteViewModel) => string;
  onSelect: (id: string) => void;
  onViewRoster: () => void;
}
const TeamInviteListRow: React.FC<TeamInviteListRowProps> = ({
  invite,
  getPendingLabel,
  onSelect,
  onViewRoster,
}) => (
  <div className="border border-gray-200 rounded-lg p-4 mb-3 hover:shadow-md">
    <div
      className="flex items-center justify-between cursor-pointer"
      onClick={() => onSelect(invite.participantId ?? "")}
    >
      <div>
        <h4 className="font-semibold text-gray-900">{invite.participantName}</h4>
        <p className="text-sm text-gray-600">
          Requested{" "}
          {new Date(invite.createdAt).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
        </p>
        {invite.status === "pending" && (
          <p className="text-xs text-amber-700 mt-0.5">{getPendingLabel(invite)}</p>
        )}
      </div>
      <StatusBadge status={invite.status || "unknown"} />
    </div>
    {invite.status === "approved" && (
      <div className="mt-3 pt-3 border-t border-gray-200">
        <button
          onClick={(e) => {
            e.stopPropagation();
            onViewRoster();
          }}
          className="text-sm text-blue-600 hover:text-blue-800 font-medium"
        >
          View Roster →
        </button>
      </div>
    )}
  </div>
);

const RegistrationsModal = forwardRef<RegistrationsModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [tournamentId, setTournamentId] = useState<string>("");
  const [tournamentName, setTournamentName] = useState<string>("");
  const [selectedInvite, setSelectedInvite] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const rosterViewModalRef = useRef<RosterViewModalRef>(null);

  const { data: invites, refetch } = useGetTournamentInvitesQuery(
    { tournamentId },
    { skip: !tournamentId }
  );

  const [respondToInvite] = useRespondToInviteMutation();
  const [deleteInvite] = useDeleteInviteMutation();

  /** All team-type invites */
  const teamInvites = useMemo(
    () => (invites ?? []).filter((i) => i.participantType !== "player"),
    [invites]
  );

  useImperativeHandle(ref, () => ({
    open: (tournId: string, tournName: string) => {
      setTournamentId(tournId);
      setTournamentName(tournName);
      setSelectedInvite(null);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
    setSelectedInvite(null);
  }

  async function handleApprove(participantId: string, name: string) {
    setIsSubmitting(true);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved: true },
      }).unwrap();
      showAlert(`Successfully approved ${name}'s registration!`, "success");
      refetch();
    } catch (error) {
      console.error("Failed to approve:", error);
      showAlert(getApiErrorMessage(error, `Failed to approve ${name}'s registration. Please try again.`), "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeny(participantId: string, name: string) {
    setIsSubmitting(true);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved: false },
      }).unwrap();
      showAlert(`Successfully denied ${name}'s registration.`, "success");
      refetch();
    } catch (error) {
      console.error("Failed to deny:", error);
      showAlert(getApiErrorMessage(error, `Failed to deny ${name}'s registration. Please try again.`), "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeleteInvite(participantId: string, name: string) {
    setIsSubmitting(true);
    try {
      await deleteInvite({ tournamentId, participantId }).unwrap();
      showAlert(`Removed ${name}'s invite. You can now re-invite them.`, "success");
      refetch();
      setSelectedInvite(null);
    } catch (error) {
      console.error("Failed to delete invite:", error);
      showAlert(getApiErrorMessage(error, "Failed to remove invite. Please try again."), "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  function getPendingLabel(invite: TournamentInviteViewModel): string {
    if (invite.tournamentManagerApproval?.status === "pending") return "Awaiting your review";
    if (invite.participantApproval?.status === "pending") return "Awaiting team response";
    return "Pending";
  }

  const selectedInviteData = invites?.find((i) => i.participantId === selectedInvite);

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert
          message={alertState.message}
          type={alertState.type}
          onClose={hideAlert}
        />
      )}
      <RosterViewModal ref={rosterViewModalRef} />
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
                  {selectedInvite ? "Registration Details" : "Team Registrations"}
                </DialogTitle>
                <button
                  onClick={close}
                  className="text-gray-400 hover:text-gray-600"
                  style={{ fontSize: "24px", lineHeight: 1 }}
                >
                  ×
                </button>
              </div>
              {selectedInvite && (
                <button
                  onClick={() => setSelectedInvite(null)}
                  className="text-blue-600 text-sm mt-2 hover:underline"
                >
                  ← Back to list
                </button>
              )}
            </div>

            {/* Content */}
            <div className="p-6 overflow-y-auto" style={{ maxHeight: "60vh" }}>
              {selectedInvite && selectedInviteData ? (
                /* ── Team detail view ── */
                <TeamDetailView
                  invite={selectedInviteData}
                  isSubmitting={isSubmitting}
                  onApprove={handleApprove}
                  onDeny={handleDeny}
                  onDelete={handleDeleteInvite}
                />
              ) : (
                /* ── List view ── */
                <>
                  {teamInvites.length > 0 ? (
                    <div>
                      {/* Team invite rows */}
                      {teamInvites.map((invite) => {
                        const id = invite.participantId ?? "";
                        return (
                          <TeamInviteListRow
                            key={id}
                            invite={invite}
                            getPendingLabel={getPendingLabel}
                            onSelect={(selId) => setSelectedInvite(selId)}
                            onViewRoster={() =>
                              rosterViewModalRef.current?.open(
                                tournamentId,
                                id,
                                invite.participantName || "Unknown Team",
                                tournamentName,
                              )
                            }
                          />
                        );
                      })}
                    </div>
                  ) : (
                    <p className="text-gray-600 text-center py-8">No team registrations yet.</p>
                  )}
                </>
              )}
            </div>
          </DialogPanel>
        </div>
      </Dialog>
    </>
  );
});

RegistrationsModal.displayName = "RegistrationsModal";

export default RegistrationsModal;
