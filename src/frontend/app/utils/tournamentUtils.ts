import { TeamGroupAffiliation, TournamentType } from "../store/serviceApi";

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
