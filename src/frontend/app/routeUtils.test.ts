import { getDefaultRedirect } from "./routeUtils";

describe("getDefaultRedirect", () => {
  test("sends a test-admin-only user to the admin page", () => {
    expect(getDefaultRedirect("user-1", ["TestAdmin"])).toBe("/admin");
  });

  test("sends a test admin who is also a referee to their referee profile", () => {
    expect(getDefaultRedirect("user-1", ["TestAdmin", "Referee"])).toBe("/referees/user-1");
  });
});
