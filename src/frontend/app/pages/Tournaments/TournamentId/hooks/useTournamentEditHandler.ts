import { useRef, useCallback } from "react";
import { AddTournamentModalRef } from "../AddTournamentModal";
import { TournamentViewModel } from "../../../../store/serviceApi";

export const useTournamentEditHandler = (tournament: TournamentViewModel | undefined) => {
  const editModalRef = useRef<AddTournamentModalRef>(null);

  const handleEdit = useCallback(() => {
    if (!tournament) return;

    editModalRef.current?.openEdit({
      id: tournament.id || "",
      name: tournament.name || "",
      description: tournament.description || "",
      startDate: tournament.startDate || "",
      endDate: tournament.endDate || "",
      registrationEndsDate: tournament.registrationEndsDate || "",
      type: tournament.type || ("" as const),
      country: tournament.country || "",
      city: tournament.city || "",
      place: tournament.place || "",
      organizer: tournament.organizer || "",
      isPrivate: tournament.isPrivate || false,
      isRegistrationOpen: tournament.isRegistrationOpen ?? true,
      bannerImageUrl: tournament.bannerImageUrl || "",
    });
  }, [tournament]);

  return {
    editModalRef,
    handleEdit,
  };
};
