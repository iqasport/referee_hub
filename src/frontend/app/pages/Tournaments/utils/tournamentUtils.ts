import { useMemo } from "react";
import { TournamentViewModel } from "../../../store/serviceApi";
import { TournamentData } from "../components/TournamentsSection";

export const convertToDisplayFormat = (t: TournamentViewModel): TournamentData => ({
  id: t.id,
  title: t.name || "",
  description: t.description || "",
  startDate: t.startDate || "",
  endDate: t.endDate || "",
  type: t.type,
  country: t.country || "",
  location: [t.place, t.city].filter(Boolean).join(", "),
  bannerImageUrl: t.bannerImageUrl || undefined,
  organizer: t.organizer || undefined,
  isPrivate: Boolean(t.isCurrentUserInvolved),
});

export const applyTypeFilter = (tournaments: TournamentViewModel[], typeFilter: string): TournamentViewModel[] => {
  if (!typeFilter) {
    return tournaments;
  }
  return tournaments.filter((t) => t.type === typeFilter);
};

export const calculatePublicTournamentCount = (
  allTournaments: TournamentViewModel[],
  typeFilter: string
): number => {
  const filtered = applyTypeFilter(allTournaments, typeFilter);
  return filtered.filter((t) => !t.isCurrentUserInvolved).length;
};

interface TournamentSections {
  publicTournaments: TournamentData[];
  privateTournaments: TournamentData[];
  totalCount: number;
}

export const useTournamentSections = (
  isAnonymous: boolean,
  filteredAllTournaments: TournamentViewModel[],
  filteredPaginatedTournaments: TournamentViewModel[]
): TournamentSections => {
  return useMemo(() => {
    if (isAnonymous) {
      return {
        publicTournaments: filteredPaginatedTournaments.map((t) => convertToDisplayFormat({
          ...t,
          isCurrentUserInvolved: false,
        })),
        privateTournaments: [],
        totalCount: filteredAllTournaments.length,
      };
    }

    // Private tournaments come from the unpaginated query (all tournaments)
    const userInvolvedTournaments = filteredAllTournaments
      .filter((t) => t.isCurrentUserInvolved)
      .map(convertToDisplayFormat);

    // Public tournaments come from the paginated query
    const otherTournaments = filteredPaginatedTournaments
      .filter((t) => !t.isCurrentUserInvolved)
      .map(convertToDisplayFormat);

    // Calculate public tournament count from all tournaments (for correct pagination)
    const publicTournamentCount = filteredAllTournaments.filter(
      (t) => !t.isCurrentUserInvolved
    ).length;

    return {
      publicTournaments: otherTournaments,
      privateTournaments: userInvolvedTournaments,
      totalCount: publicTournamentCount,
    };
  }, [isAnonymous, filteredAllTournaments, filteredPaginatedTournaments]);
};
