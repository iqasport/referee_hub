import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import { useRespondToInviteMutation } from "../../../store/serviceApi";

interface TeamInvite {
  teamId: string;
  teamName: string;
  participantId: string;
}

export interface InviteResponseModalRef {
  open: (invite: TeamInvite, tournamentId: string) => void;
}

const InviteResponseModal = forwardRef<InviteResponseModalRef>((_props, ref) => {
  const [isOpen, setIsOpen] = useState(false);
  const [invite, setInvite] = useState<TeamInvite | null>(null);
  const [tournamentId, setTournamentId] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [respondToInvite] = useRespondToInviteMutation();

  useImperativeHandle(ref, () => ({
    open: (inviteData: TeamInvite, tournId: string) => {
      setInvite(inviteData);
      setTournamentId(tournId);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
  }

  async function handleApprove() {
    if (!invite || !tournamentId) return;
    setIsSubmitting(true);

    try {
      await respondToInvite({
        tournamentId,
        participantId: invite.participantId,
        inviteResponseModel: {
          approved: true,
        },
      }).unwrap();

      console.log("Invite approved:", {
        tournamentId,
        teamId: invite.teamId,
        teamName: invite.teamName,
      });

      alert(`Successfully approved ${invite.teamName}'s registration for the tournament!`);
      close();
    } catch (error: unknown) {
      console.error("Failed to approve invite:", error);
      const errorMessage = error instanceof Error ? error.message : "Failed to approve invite. Please try again.";
      alert(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeny() {
    if (!invite || !tournamentId) return;
    setIsSubmitting(true);

    try {
      await respondToInvite({
        tournamentId,
        participantId: invite.participantId,
        inviteResponseModel: {
          approved: false,
        },
      }).unwrap();

      console.log("Invite denied:", {
        tournamentId,
        teamId: invite.teamId,
        teamName: invite.teamName,
      });

      alert(`Successfully denied ${invite.teamName}'s registration for the tournament.`);
      close();
    } catch (error: unknown) {
      console.error("Failed to deny invite:", error);
      const errorMessage = error instanceof Error ? error.message : "Failed to deny invite. Please try again.";
      alert(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Dialog
      open={isOpen && !!invite}
      as="div"
      className="relative z-50"
      onClose={close}
    >
      <div className="fixed inset-0" style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }} aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel className="relative w-full max-w-md rounded bg-white p-6 shadow-xl my-8">
          <DialogTitle as="h3" className="text-lg font-semibold text-gray-900 mb-4">
            Team Registration Request
          </DialogTitle>

          {/* Team Info */}
          <div className="bg-gray-50 rounded-lg p-4 mb-6">
            <p className="text-sm text-gray-600 mb-2">Team Name</p>
            <p className="text-lg font-semibold text-gray-900">{invite?.teamName}</p>
          </div>

          {/* Description */}
          <p className="text-sm text-gray-600 mb-6">
            Do you want to approve or deny this team&apos;s registration request for your tournament?
          </p>

          {/* Action Buttons */}
          <div className="flex gap-3">
            <button
              type="button"
              onClick={close}
              disabled={isSubmitting}
              className="px-6 py-2 text-sm font-medium rounded mr-3"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleDeny}
              disabled={isSubmitting}
              className="px-6 py-2 text-sm font-medium rounded mr-3"
            >
              {isSubmitting ? "Processing..." : "Deny"}
            </button>
            <button
              type="button"
              onClick={handleApprove}
              disabled={isSubmitting}
              className="px-6 py-2 text-sm font-medium rounded mr-3"
            >
              {isSubmitting ? "Processing..." : "Approve"}
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
});

InviteResponseModal.displayName = "InviteResponseModal";

export default InviteResponseModal;
