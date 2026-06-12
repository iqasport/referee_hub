import { useState } from "react";
import { useRespondToInviteMutation, useGetTournamentInvitesQuery } from "../../../../store/serviceApi";

interface UseInviteResponseProps {
  tournamentId: string | undefined;
  isAnonymous: boolean;
}

export const useInviteResponse = ({ tournamentId, isAnonymous }: UseInviteResponseProps) => {
  const [respondingTo, setRespondingTo] = useState<string | null>(null);

  // Query tournament invites
  const { data: invites = [], refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Mutations
  const [respondToInvite] = useRespondToInviteMutation();

  return {
    invites,
    respondingTo,
    setRespondingTo,
    respondToInvite,
    refetchInvites,
  };
};
