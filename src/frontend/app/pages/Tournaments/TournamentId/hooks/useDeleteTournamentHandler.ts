import { useCallback } from "react";
import { useDeleteTournamentMutation } from "../../../../store/serviceApi";
import { useNavigate } from "../../../../utils/navigationUtils";

interface UseDeleteTournamentHandlerProps {
  tournamentId: string | undefined;
  tournamentName: string | undefined;
  showAlert: (message: string, type: "success" | "error") => void;
}

export const useDeleteTournamentHandler = ({
  tournamentId,
  tournamentName,
  showAlert,
}: UseDeleteTournamentHandlerProps) => {
  const navigate = useNavigate();
  const [deleteTournament] = useDeleteTournamentMutation();

  const handleDelete = useCallback(async () => {
    if (!tournamentId) return;
    if (!window.confirm(`Are you sure you want to delete "${tournamentName ?? "this tournament"}"? It will be removed from view.`)) return;

    try {
      await deleteTournament({ tournamentId }).unwrap();
      navigate("/tournaments");
    } catch (error) {
      console.error("Failed to delete tournament:", error);
      showAlert("Failed to delete the tournament. Please try again.", "error");
    }
  }, [tournamentId, tournamentName, deleteTournament, navigate, showAlert]);

  return { handleDelete };
};
