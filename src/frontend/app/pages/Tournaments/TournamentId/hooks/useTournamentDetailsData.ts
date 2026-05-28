import {
  useGetManagedTeamsQuery,
  useGetParticipantsQuery,
  useDeleteTournamentMutation,
} from "../../../../store/serviceApi";
import { useCurrentUser } from "../../../../CurrentUserContext";
import { useTournamentManagerCheck } from "./useTournamentManagerCheck";
import { useTournamentData } from "./useTournamentData";
import { useInviteResponse } from "./useInviteResponse";

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
  const { currentUser, isAnonymous, isLoading: isCurrentUserLoading } = useCurrentUser();

  // Get tournament data (handles both authenticated and public queries)
  const { tournament, isLoading: isTournamentLoading, isError: isTournamentError } =
    useTournamentData(tournamentId, isAnonymous, isCurrentUserLoading);

  // Get manager status and managers list
  const { isTournamentManager, managers, managersError } =
    useTournamentManagerCheck(tournamentId, currentUser);

  // Get invite response functionality
  const { invites, respondToInvite, refetchInvites } =
    useInviteResponse({ tournamentId, isAnonymous });

  // Query managed teams
  const { data: managedTeamsData = [] } = useGetManagedTeamsQuery(undefined, {
    skip: isAnonymous,
  });

  // Query participants
  const { data: participants = [], refetch: refetchParticipants } = useGetParticipantsQuery(
    { tournamentId: tournamentId || "" },
    { skip: !tournamentId || isAnonymous }
  );

  // Mutations
  const [deleteTournament] = useDeleteTournamentMutation();

  const isLoading = isCurrentUserLoading || isTournamentLoading;
  const isError = isTournamentError;

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
