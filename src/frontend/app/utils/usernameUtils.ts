// Mirrors ManagementHub.Models.Domain.User.UsernameRules on the backend.
export const USERNAME_MIN_LENGTH = 3;
export const USERNAME_MAX_LENGTH = 30;
const usernameRegex = /^[a-z0-9][a-z0-9._-]{1,28}[a-z0-9]$/;
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const userIdRegex = /^U_/i;

export const USERNAME_RULES_DESCRIPTION =
  "3-30 characters: lowercase letters, digits, '.', '_' or '-'. Must start and end with a letter or digit.";

export const normalizeUsername = (value: string): string => value.trim().toLowerCase();

export const isValidUsername = (value: string): boolean => usernameRegex.test(normalizeUsername(value));

export const isValidEmail = (value: string): boolean => emailRegex.test(value.trim());

/** Inputs containing '@' are treated as emails, everything else as a username. */
export const isValidEmailOrUsername = (value: string): boolean =>
  value.includes("@") ? isValidEmail(value) : isValidUsername(value);

/** Whether a profile route parameter is a username rather than "me" or a user id. */
export const isUsernameRouteParam = (value: string | undefined): value is string =>
  !!value && value !== "me" && !userIdRegex.test(value);

export const buildProfileLink = (username: string): string =>
  `${window.location.origin}/referees/${encodeURIComponent(username)}`;
