import {
  isUsernameRouteParam,
  isValidEmailOrUsername,
  isValidUsername,
  normalizeUsername,
} from "./usernameUtils";

describe("usernameUtils", () => {
  describe("isValidUsername", () => {
    it.each(["abc", "andres.vargas", "jimmy_ref-2", "Andres.Vargas", "  padded.name  "])(
      "accepts %s",
      (value) => {
        expect(isValidUsername(value)).toBe(true);
      }
    );

    it.each(["", "ab", ".abc", "abc.", "has space", "a@b.com", "a".repeat(31)])(
      "rejects %s",
      (value) => {
        expect(isValidUsername(value)).toBe(false);
      }
    );
  });

  describe("normalizeUsername", () => {
    it("trims and lowercases", () => {
      expect(normalizeUsername("  Andres.Vargas ")).toBe("andres.vargas");
    });
  });

  describe("isValidEmailOrUsername", () => {
    it("accepts a valid email", () => {
      expect(isValidEmailOrUsername("someone@example.com")).toBe(true);
    });

    it("accepts a valid username", () => {
      expect(isValidEmailOrUsername("jimmy.referee")).toBe(true);
    });

    it("rejects a malformed email", () => {
      expect(isValidEmailOrUsername("someone@example")).toBe(false);
    });

    it("rejects an invalid username", () => {
      expect(isValidEmailOrUsername("x!")).toBe(false);
    });
  });

  describe("isUsernameRouteParam", () => {
    it("treats me and user ids as non-usernames", () => {
      expect(isUsernameRouteParam("me")).toBe(false);
      expect(isUsernameRouteParam("U_abcdefghijklmnopqrstuvwxyz")).toBe(false);
      expect(isUsernameRouteParam(undefined)).toBe(false);
    });

    it("treats other values as usernames", () => {
      expect(isUsernameRouteParam("jimmy.referee")).toBe(true);
    });
  });
});
