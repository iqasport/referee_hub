import {
  NgbTeamViewModel,
  TeamGroupAffiliation,
  TournamentType,
} from "../store/serviceApi";

/**
 * Returns the set of TeamGroupAffiliation values that are eligible to
 * participate in a tournament of the given type.
 *
 * - National  → national teams only
 * - Club      → community and university teams
 * - Youth     → youth teams only
 * - Fantasy   → all team types (null = no restriction)
 */
export function getEligibleAffiliations(
  tournamentType: TournamentType | undefined | null
): TeamGroupAffiliation[] | null {
  switch (tournamentType) {
    case "National":
      return ["national"];
    case "Club":
      return ["community", "university"];
    case "Youth":
      return ["youth"];
    case "Fantasy":
    default:
      return null; // no restriction
  }
}

/**
 * Returns true when a team with the given groupAffiliation is eligible
 * for a tournament of the given type.
 */
export function isTeamEligible(
  teamAffiliation: TeamGroupAffiliation | undefined | null,
  tournamentType: TournamentType | undefined | null
): boolean {
  const eligible = getEligibleAffiliations(tournamentType);
  if (eligible === null) return true; // Fantasy or unknown — allow all
  if (!teamAffiliation) return false;
  return eligible.includes(teamAffiliation);
}

/**
 * Human-readable label for what team types are eligible.
 */
export function eligibilityLabel(tournamentType: TournamentType | undefined | null): string {
  switch (tournamentType) {
    case "National":
      return "National teams only";
    case "Club":
      return "Community and University teams only";
    case "Youth":
      return "Youth teams only";
    case "Fantasy":
      return "All team types";
    default:
      return "All team types";
  }
}

/** Shape of an ASP.NET Core ProblemDetails error response. */
interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

/** RTK Query FetchBaseQueryError shape (subset we care about). */
interface FetchError {
  status: number | string;
  data?: unknown;
  error?: string;
}

/**
 * Extracts a human-readable message from an RTK Query error.
 * Falls back to `fallback` when no structured message is available.
 *
 * Handles:
 *  - ASP.NET Core ProblemDetails (detail, title, errors)
 *  - Plain string error body
 *  - Network / serialisation errors
 */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (!error) return fallback;

  const fe = error as FetchError;

  // Network / CORS / parse error from RTK Query (status === "FETCH_ERROR" etc.)
  if (typeof fe.status === "string" && fe.error) {
    return fe.error;
  }

  if (fe.data) {
    if (typeof fe.data === "string") return fe.data || fallback;

    const pd = fe.data as ProblemDetails;

    // Flatten validation errors into a readable list
    if (pd.errors) {
      const messages = Object.values(pd.errors).flat();
      if (messages.length > 0) return messages.join(" ");
    }

    if (pd.detail) return pd.detail;
    if (pd.title) return pd.title;
  }

  return fallback;
}

/** Shape of a team in the UI. */
type Team = NgbTeamViewModel;

/** Shape of an invite in the UI. */
interface Invite {
  participantId?: string;
}

/** Shape of a participant in the UI. */
interface Participant {
  teamId?: string;
}

/**
 * Collects the set of team IDs that are unavailable for inviting
 * (already have invites or are participants).
 */
export function getUnavailableTeamIds(
  invites?: Invite[] | null,
  participants?: Participant[] | null
): Set<string> {
  const ids = new Set<string>();

  if (invites) {
    invites.forEach((invite) => {
      if (invite.participantId) {
        ids.add(invite.participantId);
      }
    });
  }

  if (participants) {
    participants.forEach((participant) => {
      if (participant.teamId) {
        ids.add(participant.teamId);
      }
    });
  }

  return ids;
}

/**
 * Filters and searches a list of teams, applying all eligibility rules.
 * Returns teams that:
 * - Have a valid teamId
 * - Are not unavailable (already invited/participating)
 * - Are not inactive
 * - Are eligible for the tournament type
 * - Match the search filter (if provided)
 */
export function filterAndSearchTeams(
  teams: Team[] | undefined,
  unavailableTeamIds: Set<string>,
  searchFilter: string,
  tournamentType: TournamentType | undefined | null
): Team[] {
  if (!teams) return [];

  return teams.filter((team: Team) => {
    if (!team.teamId) return false;
    if (unavailableTeamIds.has(team.teamId)) return false;
    if (team.status === "inactive") return false;
    if (!isTeamEligible(team.groupAffiliation, tournamentType)) return false;

    if (searchFilter) {
      const name = (team.name || "").toLowerCase();
      return name.includes(searchFilter.toLowerCase());
    }

    return true;
  });
}

/**
 * Returns CSS styles for an invite status badge.
 */
export function getTeamStatusStyle(status?: string): { bg: string; color: string } {
  if (status === "pending") {
    return { bg: "#fef3c7", color: "#92400e" };
  } else if (status === "approved") {
    return { bg: "#d1fae5", color: "#065f46" };
  } else {
    return { bg: "#fee2e2", color: "#991b1b" };
  }
}
