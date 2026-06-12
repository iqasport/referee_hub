import { useCallback } from "react";
import { useRespondToInviteMutation } from "../../../../store/serviceApi";

interface UseRespondToInviteHandlerProps {
  tournamentId: string | undefined;
  respondingTo: string | null;
  setRespondingTo: (value: string | null) => void;
  refetchInvites: () => void;
  showAlert: (message: string, type: "success" | "error") => void;
  respondToInviteMutation: any;
}

export const useRespondToInviteHandler = ({
  tournamentId,
  respondingTo,
  setRespondingTo,
  refetchInvites,
  showAlert,
  respondToInviteMutation,
}: UseRespondToInviteHandlerProps) => {
  const handleRespondToInvite = useCallback(
    async (participantId: string, approved: boolean) => {
      if (!tournamentId) return;

      setRespondingTo(participantId);
      try {
        await respondToInviteMutation({
          tournamentId,
          participantId,
          inviteResponseModel: { approved },
        }).unwrap();

        showAlert(approved ? "Successfully accepted the invite!" : "Invite declined.", "success");
        refetchInvites();
      } catch (error) {
        console.error("Failed to respond to invite:", error);
        showAlert("Failed to respond. Please try again.", "error");
      } finally {
        setRespondingTo(null);
      }
    },
    [tournamentId, respondToInviteMutation, setRespondingTo, refetchInvites, showAlert]
  );

  return { handleRespondToInvite };
};
