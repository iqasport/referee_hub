import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle, useRef } from "react";
import React from "react";
import {
  InviteStatus,
  useGetParticipantsQuery,
  useDeleteInviteMutation,
  useGetTournamentInvitesQuery,
  useRemoveParticipantMutation,
  useRespondToInviteMutation,
  TournamentInviteViewModel,
} from "../../../store/serviceApi";
import StatusBadge from "../../../components/StatusBadge";
import ActionButtonPair from "../../../components/ActionButtonPair";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";
import RosterViewModal, { RosterViewModalRef } from "./RosterViewModal";

export interface RegistrationsModalRef {
  open: (tournamentId: string, tournamentName: string) => void;
}

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
  const { data: participants } = useGetParticipantsQuery(
    { tournamentId },
    { skip: !tournamentId }
  );

  const [respondToInvite] = useRespondToInviteMutation();
  const [removeParticipant] = useRemoveParticipantMutation();
  const [deleteInvite] = useDeleteInviteMutation();

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

  async function handleInviteResponse(
    participantId: string,
    teamName: string,
    approved: boolean
  ) {
    setIsSubmitting(true);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved },
      }).unwrap();
      showAlert(
        approved
          ? `Successfully approved ${teamName}'s registration!`
          : `Successfully denied ${teamName}'s registration.`,
        "success"
      );
      refetch();
      setSelectedInvite(null);
    } catch (error) {
      console.error("Failed to submit invite response:", error);
      showAlert(
        approved ? "Failed to approve. Please try again." : "Failed to deny. Please try again.",
        "error"
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleRemoveEntry(
    participantId: string,
    participantName: string,
    status: InviteStatus | undefined
  ) {
    setIsSubmitting(true);
    try {
      if (status === "approved") {
        await removeParticipant({
          tournamentId,
          teamId: participantId,
        }).unwrap();
        showAlert(`Successfully removed ${participantName} from the tournament.`, "success");
      } else {
        await deleteInvite({
          tournamentId,
          participantId,
        }).unwrap();
        showAlert(`Successfully removed invite for ${participantName}.`, "success");
      }
      refetch();
      setSelectedInvite(null);
    } catch (error) {
      console.error("Failed to remove entry:", error);
      showAlert("Failed to remove. Please try again.", "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  const selectedInviteData = invites?.find((i) => i.participantId === selectedInvite);

  function getPendingLabel(invite: TournamentInviteViewModel): string {
    if (invite.tournamentManagerApproval?.status === "pending") return "Awaiting your review";
    if (invite.participantApproval?.status === "pending") return "Awaiting team response";
    return "Pending";
  }

  function isApprovedInvite(status: InviteStatus | undefined): boolean {
    return status === "approved";
  }

  function isCurrentParticipant(participantId: string | null | undefined): boolean {
    if (!participantId || !participants) {
      return false;
    }

    return participants.some((participant) => participant.teamId === participantId);
  }

  function shouldRemoveParticipant(
    participantId: string | null | undefined,
    status: InviteStatus | undefined
  ): boolean {
    return isApprovedInvite(status) && isCurrentParticipant(participantId);
  }

  function getRemoveLabel(status: InviteStatus | undefined, detailView: boolean): string {
    if (isApprovedInvite(status)) {
      return detailView ? "Remove From Tournament" : "Remove Team";
    }

    return "Delete Invite";
  }

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
              // Detail view
              <div>
                {/* Team Name */}
                <div className="bg-gray-50 rounded-lg p-4 mb-4">
                  <p className="text-sm text-gray-600 mb-1">Team Name</p>
                  <p className="text-lg font-semibold text-gray-900">
                    {selectedInviteData.participantName}
                  </p>
                </div>

                {/* Request Date */}
                <div className="bg-gray-50 rounded-lg p-4 mb-4">
                  <p className="text-sm text-gray-600 mb-1">Request Submitted</p>
                  <p className="text-sm font-semibold text-gray-900">
                    {new Date(selectedInviteData.createdAt).toLocaleDateString("en-US", {
                      month: "short",
                      day: "numeric",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </p>
                </div>

                {/* Status */}
                <div className="bg-gray-50 rounded-lg p-4 mb-6">
                  <p className="text-sm text-gray-600 mb-2">Status</p>
                  <div className="flex items-center">
                    <StatusBadge status={selectedInviteData.status || "unknown"} />
                    {selectedInviteData.status !== "pending" &&
                      selectedInviteData.tournamentManagerApproval?.date && (
                        <span className="text-xs text-gray-600 ml-2">
                          on{" "}
                          {new Date(
                            selectedInviteData.tournamentManagerApproval.date
                          ).toLocaleDateString("en-US", {
                            month: "short",
                            day: "numeric",
                          })}
                        </span>
                      )}
                  </div>
                </div>

                {/* Actions for pending - only show if tournament manager approval is pending */}
                {selectedInviteData.status === "pending" &&
                  selectedInviteData.tournamentManagerApproval?.status === "pending" && (
                    <ActionButtonPair
                      onAccept={() =>
                        handleInviteResponse(
                          selectedInviteData.participantId,
                          selectedInviteData.participantName || "",
                          true
                        )
                      }
                      onDecline={() =>
                        handleInviteResponse(
                          selectedInviteData.participantId,
                          selectedInviteData.participantName || "",
                          false
                        )
                      }
                      isLoading={isSubmitting}
                      acceptLabel="Approve"
                      declineLabel="Deny"
                      loadingLabel="Processing..."
                    />
                  )}

                {/* Info message when waiting for team to respond */}
                {selectedInviteData.status === "pending" &&
                  selectedInviteData.tournamentManagerApproval?.status === "approved" &&
                  selectedInviteData.participantApproval?.status === "pending" && (
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                      <p className="text-sm text-blue-800">
                        ⏳ Waiting for the team to accept or decline this invitation.
                      </p>
                    </div>
                  )}

                <div className="mt-4">
                  <button
                    onClick={() =>
                      handleRemoveEntry(
                        selectedInviteData.participantId,
                        selectedInviteData.participantName || "this team",
                        shouldRemoveParticipant(
                          selectedInviteData.participantId,
                          selectedInviteData.status
                        )
                          ? "approved"
                          : "pending"
                      )
                    }
                    className="px-4 py-2 rounded-md bg-red-600 text-white text-sm font-medium hover:bg-red-700 disabled:opacity-50"
                    disabled={isSubmitting}
                  >
                    {isSubmitting
                      ? "Removing..."
                      : getRemoveLabel(
                          shouldRemoveParticipant(
                            selectedInviteData.participantId,
                            selectedInviteData.status
                          )
                            ? "approved"
                            : "pending",
                          true
                        )}
                  </button>
                </div>
              </div>
            ) : (
              // List view
              <>
                {invites && invites.length > 0 ? (
                  <div>
                    {invites.map((invite) => (
                      <div
                        key={invite.participantId}
                        className="border border-gray-200 rounded-lg p-4 mb-3 hover:shadow-md"
                      >
                        <div 
                          className="flex items-center justify-between cursor-pointer"
                          onClick={() => setSelectedInvite(invite.participantId)}
                        >
                          <div>
                            <h4 className="font-semibold text-gray-900">
                              {invite.participantName}
                            </h4>
                            <p className="text-sm text-gray-600">
                              Requested{" "}
                              {new Date(invite.createdAt).toLocaleDateString("en-US", {
                                month: "short",
                                day: "numeric",
                                year: "numeric",
                              })}
                            </p>
                            {invite.status === "pending" && (
                              <p className="text-xs text-amber-700 mt-0.5">{getPendingLabel(invite)}</p>
                            )}
                          </div>
                          <StatusBadge status={invite.status || "unknown"} />
                        </div>
                        <div className="mt-3 pt-3 border-t border-gray-200">
                          <div className="flex items-center gap-4">
                            {shouldRemoveParticipant(invite.participantId, invite.status) && (
                              <button
                                onClick={(e) => {
                                  e.stopPropagation();
                                  rosterViewModalRef.current?.open(
                                    tournamentId,
                                    invite.participantId,
                                    invite.participantName || "Unknown Team",
                                    tournamentName
                                  );
                                }}
                                className="text-sm text-blue-600 hover:text-blue-800 font-medium"
                              >
                                View Roster →
                              </button>
                            )}
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                handleRemoveEntry(
                                  invite.participantId,
                                  invite.participantName || "this team",
                                  shouldRemoveParticipant(invite.participantId, invite.status)
                                    ? "approved"
                                    : "pending"
                                );
                              }}
                              className="text-sm text-red-600 hover:text-red-800 font-medium disabled:opacity-50"
                              disabled={isSubmitting}
                            >
                              {isSubmitting
                                ? "Removing..."
                                : getRemoveLabel(
                                    shouldRemoveParticipant(invite.participantId, invite.status)
                                      ? "approved"
                                      : "pending",
                                    false
                                  )}
                            </button>
                          </div>
                        </div>
                      </div>
                    ))}
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
