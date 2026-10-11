export const getDefaultRedirect = (userId: string | undefined, roles: string[], ownedNgbId?: string): string | undefined => {
  if (roles.includes("IqaAdmin")) return "/admin";
  if (roles.includes("NgbAdmin") && ownedNgbId) return `/national_governing_bodies/${ownedNgbId}`;
  if (roles.includes("Referee")) return `/referees/${userId}`;
  if (roles.includes("TestAdmin")) return "/admin";

  return undefined;
};
