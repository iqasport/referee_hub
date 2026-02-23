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
