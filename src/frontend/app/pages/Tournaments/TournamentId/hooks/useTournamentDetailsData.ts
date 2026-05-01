import { useState } from "react";
import {
  useGetTournamentQuery,
  useGetPublicTournamentQuery,
  useGetTournamentManagersQuery,
  useGetCurrentUserQuery,
  useGetTournamentInvitesQuery,
  useGetManagedTeamsQuery,
  useGetParticipantsQuery,
  useRespondToInviteMutation,
  useDeleteTournamentMutation,
} from "../../../../store/serviceApi";

interface UseTournamentDetailsDataReturn {
  currentUser: any;
  tournament: any;
  managers: any[];
  invites: any[];
  participants: any[];
  managedTeamsData: any[];
  isAnonymous: boolean;
  isTournamentManager: boolean;
  isLoading: boolean;
  isError: boolean;
  managersError: boolean;
  respondToInvite: any;
  deleteTournament: any;
  refetchInvites: () => void;
  refetchParticipants: () => void;
}

export const useTournamentDetailsData = (tournamentId: string | undefined): UseTournamentDetailsDataReturn => {
  const [respondingTo, setRespondingTo] = useState<string | null>(null);

  // Query for current user
  const {
    data: currentUser,
    isLoading: isCurrentUserLoading,
    isError: isCurrentUserError,
  } = useGetCurrentUserQuery();

  const isAnonymous = !isCurrentUserLoading && (isCurrentUserError || !currentUser);
  const shouldUseAuthenticatedTournamentQuery = !isCurrentUserLoading && !isAnonymous;

  // Query for authenticated tournament view
  const {
    data: authenticatedTournament,
    isLoading: isLoadingAuthenticatedTournament,
    isError: isAuthenticatedTournamentError,
  } = useGetTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || !shouldUseAuthenticatedTournamentQuery }
  );

  // Query for public tournament view (anonymous users)
  const {
    data: publicTournament,
    isLoading: isLoadingPublicTournament,
    isError: isPublicTournamentError,
  } = useGetPublicTournamentQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || shouldUseAuthenticatedTournamentQuery }
  );

  // Determine which tournament data to use
  const tournament = isAnonymous
    ? (publicTournament ? { ...publicTournament, isCurrentUserInvolved: false } : undefined)
    : authenticatedTournament;
  const isLoading = isCurrentUserLoading || isLoadingAuthenticatedTournament || isLoadingPublicTournament;
  const isError = isAnonymous ? isPublicTournamentError : isAuthenticatedTournamentError;

  // Query managed teams
  const { data: managedTeamsData = [] } = useGetManagedTeamsQuery(undefined, {
    skip: isAnonymous,
  });

  // Check if user is a tournament manager for this specific tournament
  const isTournamentManagerOfThis = currentUser?.roles?.some((role: any) => {
    if (role.roleType !== "TournamentManager") return false;
    if (role.tournament === "ANY") return true;
    if (Array.isArray(role.tournament)) {
      return role.tournament.includes(tournamentId);
    }
    return role.tournament === tournamentId;
  });

  // Query tournament managers
  const shouldFetchManagers = Boolean(tournamentId && isTournamentManagerOfThis);
  const { data: managers = [], isError: managersError } = useGetTournamentManagersQuery(
    { tournamentId: tournamentId || "" },
    { skip: !shouldFetchManagers }
  );

  // Check if current user is a manager
  const isTournamentManager = !managersError && currentUser?.userId && managers
    ? managers.some((manager) => manager.id === currentUser.userId)
    : false;

  // Query tournament invites
  const { data: invites = [], refetch: refetchInvites } = useGetTournamentInvitesQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Query participants
  const { data: participants = [], refetch: refetchParticipants } = useGetParticipantsQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Mutations
  const [respondToInvite] = useRespondToInviteMutation();
  const [deleteTournament] = useDeleteTournamentMutation();

  return {
    currentUser,
    tournament,
    managers,
    invites,
    participants,
    managedTeamsData,
    isAnonymous,
    isTournamentManager,
    isLoading,
    isError,
    managersError,
    respondToInvite,
    deleteTournament,
    refetchInvites,
    refetchParticipants,
  };
};
