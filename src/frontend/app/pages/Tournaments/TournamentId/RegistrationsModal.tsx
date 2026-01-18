import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import {
  useGetTournamentInvitesQuery,
  useRespondToInviteMutation,
} from "../../../store/serviceApi";

export interface RegistrationsModalRef {
  open: (tournamentId: string) => void;
}

const RegistrationsModal = forwardRef<RegistrationsModalRef>((_props, ref) => {
  const [isOpen, setIsOpen] = useState(false);
  const [tournamentId, setTournamentId] = useState<string>("");
  const [selectedInvite, setSelectedInvite] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { data: invites, refetch } = useGetTournamentInvitesQuery(
    { tournamentId },
    { skip: !tournamentId }
  );

  const [respondToInvite] = useRespondToInviteMutation();

  useImperativeHandle(ref, () => ({
    open: (tournId: string) => {
      setTournamentId(tournId);
      setSelectedInvite(null);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
    setSelectedInvite(null);
  }

  async function handleApprove(participantId: string, teamName: string) {
    setIsSubmitting(true);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved: true },
      }).unwrap();
      alert(`Successfully approved ${teamName}'s registration!`);
      refetch();
      setSelectedInvite(null);
    } catch (error) {
      console.error("Failed to approve:", error);
      alert("Failed to approve. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeny(participantId: string, teamName: string) {
    setIsSubmitting(true);
    try {
      await respondToInvite({
        tournamentId,
        participantId,
        inviteResponseModel: { approved: false },
      }).unwrap();
      alert(`Successfully denied ${teamName}'s registration.`);
      refetch();
      setSelectedInvite(null);
    } catch (error) {
      console.error("Failed to deny:", error);
      alert("Failed to deny. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const getStatusStyle = (status?: string) => {
    if (status === "pending") {
      return { bg: "#fef3c7", color: "#92400e", border: "#fde68a" };
    } else if (status === "approved") {
      return { bg: "#d1fae5", color: "#065f46", border: "#a7f3d0" };
    } else {
      return { bg: "#fee2e2", color: "#991b1b", border: "#fecaca" };
    }
  };

  const selectedInviteData = invites?.find((i) => i.participantId === selectedInvite);

  return (
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
                    {(() => {
                      const statusStyle = getStatusStyle(selectedInviteData.status);
                      return (
                        <>
                          <span
                            className="px-3 py-1 rounded-full text-sm font-semibold"
                            style={{ backgroundColor: statusStyle.bg, color: statusStyle.color }}
                          >
                            {selectedInviteData.status
                              ? selectedInviteData.status.charAt(0).toUpperCase() +
                                selectedInviteData.status.slice(1)
                              : "Unknown"}
                          </span>
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
                        </>
                      );
                    })()}
                  </div>
                </div>

                {/* Actions for pending */}
                {selectedInviteData.status === "pending" && (
                  <div className="flex" style={{ gap: "12px" }}>
                    <button
                      onClick={() =>
                        handleDeny(
                          selectedInviteData.participantId,
                          selectedInviteData.participantName || ""
                        )
                      }
                      disabled={isSubmitting}
                      className="px-4 py-2 text-sm font-medium rounded"
                      style={{
                        flex: 1,
                        backgroundColor: "#dc2626",
                        color: "white",
                        opacity: isSubmitting ? 0.5 : 1,
                        cursor: isSubmitting ? "not-allowed" : "pointer",
                      }}
                    >
                      {isSubmitting ? "Processing..." : "Deny"}
                    </button>
                    <button
                      onClick={() =>
                        handleApprove(
                          selectedInviteData.participantId,
                          selectedInviteData.participantName || ""
                        )
                      }
                      disabled={isSubmitting}
                      className="px-4 py-2 text-sm font-medium rounded"
                      style={{
                        flex: 1,
                        backgroundColor: "#16a34a",
                        color: "white",
                        opacity: isSubmitting ? 0.5 : 1,
                        cursor: isSubmitting ? "not-allowed" : "pointer",
                      }}
                    >
                      {isSubmitting ? "Processing..." : "Approve"}
                    </button>
                  </div>
                )}
              </div>
            ) : (
              // List view
              <>
                {invites && invites.length > 0 ? (
                  <div>
                    {invites.map((invite) => {
                      const statusStyle = getStatusStyle(invite.status);
                      return (
                        <div
                          key={invite.participantId}
                          className="border rounded-lg p-4 mb-3 cursor-pointer hover:shadow-md"
                          style={{ borderColor: statusStyle.border }}
                          onClick={() => setSelectedInvite(invite.participantId)}
                        >
                          <div className="flex items-center justify-between">
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
                            </div>
                            <span
                              className="px-3 py-1 rounded-full text-sm font-semibold"
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
                ) : (
                  <p className="text-gray-600 text-center py-8">No team registrations yet.</p>
                )}
              </>
            )}
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
});

RegistrationsModal.displayName = "RegistrationsModal";

export default RegistrationsModal;
