import { useMemo } from "react";
import { TournamentViewModel } from "../../../../store/serviceApi";

export const useIsRegistrationClosed = (tournament: TournamentViewModel | undefined): boolean => {
  return useMemo(() => {
    if (!tournament) return false;

    // Check manual closure first (field may not exist if migration not applied)
    if (tournament.isRegistrationOpen === false) {
      return true;
    }

    // Check if registration end date has passed
    if (tournament.registrationEndsDate) {
      const regEndsDate = new Date(tournament.registrationEndsDate);
      const today = new Date();
      // Reset hours to compare at day level
      regEndsDate.setHours(0, 0, 0, 0);
      today.setHours(0, 0, 0, 0);
      if (today > regEndsDate) {
        return true;
      }
    } else if (tournament.startDate) {
      // Fall back to start date if no registration end date
      const startDate = new Date(tournament.startDate);
      const today = new Date();
      // Reset hours to compare at day level
      startDate.setHours(0, 0, 0, 0);
      today.setHours(0, 0, 0, 0);
      if (today > startDate) {
        return true;
      }
    }

    return false;
  }, [tournament?.isRegistrationOpen, tournament?.registrationEndsDate, tournament?.startDate]);
};
